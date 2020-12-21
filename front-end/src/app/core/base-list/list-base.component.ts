import { AfterViewInit, Directive, EventEmitter, OnDestroy, ViewChild } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort, MatSortable, SortDirection } from '@angular/material/sort';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { AppConfig } from '@core/app.config';
import { ListRequest } from '@core/models/common/ListRequest';
import { BaseApiService } from '@core/services/base-api.service';
import { nameOf } from '@utils/nameof';
import { filter as arrFilter } from 'lodash-es';
import { BehaviorSubject, merge, Subscription } from 'rxjs';
import { debounceTime, delay, distinctUntilChanged, filter, tap } from 'rxjs/operators';
import { PaginatedDataSource } from './paginated-datasource';

// Basic version of generic list.
@Directive()
export abstract class ListBaseComponent<TListItemDto, TFilter extends ListRequest> implements AfterViewInit, OnDestroy {
	@ViewChild(MatPaginator)
	paginator!: MatPaginator;
	@ViewChild(MatSort, { static: false })
	sort!: MatSort;

	// The set of filters for the list (not including `ListRequest` properties)
	get filter(): Partial<TFilter> {
		return this._filter.getValue();
	}
	set filter(val: Partial<TFilter>) {
		this._filter.next(val); // Don't check if 'val' is different, because the consumers use it by reference meaning they change the original object
	}

	// Displayed columns. Override this property if you need conditional hiding of some columns
	get displayedColumns(): string[] {
		return this.defaultDisplayedColumns;
	}
	get pageSizeOptions(): number[] {
		return AppConfig.PageSizeOptions;
	}
	pageSize: number = AppConfig.PageSize;

	get dataSource(): PaginatedDataSource<TListItemDto, TFilter> {
		return this._dataSource;
	}
	private _dataSource: PaginatedDataSource<TListItemDto, TFilter>;

	protected subscriptions = new Subscription();

	private _filter: BehaviorSubject<Partial<TFilter>>;
	private _clearSort: boolean = false;
	// This is used for requested page (from URL) only, bind to the value from the response in the UI
	private _pageIndex: number = 0;
	private _requests = new EventEmitter<TFilter>();

	constructor(
		protected router: Router,
		protected activatedRoute: ActivatedRoute,
		/* The service for requesting the list from API */
		protected service: BaseApiService,
		/* Default visible columns */
		protected defaultDisplayedColumns: string[],
		/* Instance of the class for parsing filters from the Query String (doesn't need to include properties from `ListRequest`) */
		defaultFilter: Partial<TFilter>
	) {
		this._filter = new BehaviorSubject<Partial<TFilter>>(defaultFilter);
		this._dataSource = new PaginatedDataSource<TListItemDto, TFilter>(this.service, this._requests);
	}

	// Subscribe for filter triggers after the nested components get initialised (must be AfterViewInit, instead of ngOnInit).
	// If we were using 'BehaviorSubject' for 'triggers', then the current value'd have been emitted in the subscriber,
	// but we're using 'EventEmitter', so we expect that 'DataSource.connect()' is called and have subscribed for events
	ngAfterViewInit() {
		// Disable the 3rd state of sorting (the unsorted state)
		this.sort.disableClear = true;

		// Initialise the Page Index/Size and Sorting from the QueryString
		this.subscriptions.add(
			this.activatedRoute.queryParamMap
				.pipe(
					distinctUntilChanged(),
					// Make sure we modify the page/sort properties (and receive the data) after a processing delay to avoid
					// ExpressionChangedAfterItHasBeenCheckedError.
					// This seems like a poor design, but given that we rely on the view children to get the sort properties
					// (the html defines the default sort,) I don't see how we can avoid it.
					delay(0)
				)
				.subscribe(params => {
					this._pageIndex = +(params.get(nameOf<ListRequest>('pageIndex')) || 0);
					this.pageSize = +(params.get(nameOf<ListRequest>('pageSize')) || AppConfig.PageSize);

					// If the list shows results of a search, then no sorting order must be applied
					if (!!params.get('search')) this._clearSort = true;

					// Set grid Sorting
					if (!!this.sort) {
						if (params.has(nameOf<ListRequest>('orderBy'))) {
							this.sort.active = params.get(nameOf<ListRequest>('orderBy')) || '';
							this.sort.direction = (params.get(nameOf<ListRequest>('orderDirection')) || '') as SortDirection;
						}
					}

					// Apply Query String parameters to the filter
					this.filter = this.getFilterFromQueryString(params);

					this.refreshList();
				})
		);

		// List of all triggers, which can cause refreshing data in the grid
		// Reset back to the first page if we change filters (anything, except the page number)
		const triggers = [
			this.paginator.page.pipe(
				tap((page: PageEvent) => {
					if (!!page) {
						this._pageIndex = page.pageSize !== this.pageSize ? 0 : page.pageIndex;
						this.pageSize = page.pageSize;
						this.paginationHandler(page);
					}
				})
			),
			this.sort.sortChange.pipe(
				tap(() => {
					this._pageIndex = 0;
				})
			),
			this._filter.pipe(
				filter(Boolean),
				tap(() => (this._pageIndex = 0))
			),
		];

		this.subscriptions.add(
			merge(...arrFilter(triggers, Boolean))
				.pipe(
					distinctUntilChanged(),
					// A nasty way to wait for all of the components to finish updating the filter before refreshing the page
					debounceTime(100)
				)
				.subscribe(() => {
					// If we have changed the search filter then clear the sorting on the next request
					if (this._clearSort) {
						this._clearSort = false;
						this.clearSorting();
					}
					// Get merged filter from all the Query Parameters
					const accruedFilter = this.mergeFilters();
					// Update the QueryString
					this.router.navigate([], {
						queryParams: accruedFilter,
						relativeTo: this.activatedRoute,
					});
				})
		);
	}

	ngOnDestroy() {
		this.subscriptions.unsubscribe();
	}

	clearSorting(): void {
		this.sort.sort({ id: '', start: 'asc', disableClear: false } as MatSortable);
	}

	isColumnVisible(columnName: string): boolean {
		return this.displayedColumns.indexOf(columnName) !== -1;
	}

	refreshList(): void {
		const userFilter = this.mergeFilters();
		// Update the Data Source
		this._requests.emit(userFilter);
	}

	protected subscribeToEntityCreationNotificationEvent(event: EventEmitter<void>): void {
		this.subscriptions.add(event.subscribe(() => this.refreshList()));
	}

	protected paginationHandler(page: PageEvent): void {
		// Implementation in 'document-list.component.ts'
	}

	// Build filtering for the list
	protected mergeFilters(): TFilter {
		const accruedFilter: TFilter = Object.assign(
			{} as TFilter,
			this.filter,
			//this.filterComponent ? this.filterComponent.filter : {},
			{
				pageIndex: this._pageIndex,
				pageSize: this.pageSize,
			},
			this.sort && this.sort.active ? { orderBy: this.sort.active, orderDirection: this.sort.direction } : {}
		) as TFilter;
		return accruedFilter;
	}

	// Get an instance of the filter class from Query String parameters
	private getFilterFromQueryString(params: ParamMap): TFilter {
		// convert the ParamMap to an object
		const paramsObj = params.keys.reduce((obj, key) => {
			// 'params.getAll(key)' returns an array, when 'params.get(key)' - only the first value
			let value: string | string[] | null = this.filter[key as keyof TFilter] instanceof Array ? params.getAll(key) : params.get(key);
			if (key.indexOf('[') >= 0) {
				// convert dictionaries to objects
				const [parent, child] = key.split(/\[|\]/);
				value = Object.assign(obj[parent as keyof {}] || {}, { [child]: params.get(key) });
				key = parent;
			}
			return Object.assign(obj, {
				[key]: value,
			});
		}, {});
		return paramsObj as TFilter;
	}
}
