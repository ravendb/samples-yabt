import { AfterViewInit, Directive, EventEmitter, OnDestroy, ViewChild } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort, MatSortable, MatSortHeader } from '@angular/material/sort';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { ListRequest } from '@core/api-models/common/ListRequest';
import { BaseApiService } from '@core/api-services/base-api.service';
import { AppConfigService } from '@core/app-config.service';
import { nameOf } from '@utils/nameof';
import { filter as arrFilter, get, isEqual, isNil, omitBy, orderBy } from 'lodash-es';
import { merge, Subject, Subscription } from 'rxjs';
import { delay, distinctUntilChanged, filter, tap } from 'rxjs/operators';
import { PaginatedDataSource } from './paginated-datasource';

// Basic version of generic list.
/*
	Checklist for testing derived classes:
		- Changing the current page fetches the right chunk of records applying all the filters and the sorting order;
		- Sorting by columns:
			- Resets the page index;
			- Brings records in the expected order;
		- Applying filters from the filter bar:
			- Resets the page index;
			- Brings expected records;
			- If search text, then the order is reset;
		- Navigating to a filtered page in different ways:
			- Opening the page with the URL containing filters in the query string;
			- Internal navigation between the pages;
			- The browser's backward/forward button
 */
@Directive()
export abstract class ListBaseComponent<TListItemDto, TFilter extends ListRequest> implements AfterViewInit, OnDestroy {
	@ViewChild(MatPaginator)
	paginator!: MatPaginator;
	@ViewChild(MatSort)
	sort!: MatSort;

	// The set of filters for the list (not including `ListRequest` properties)
	get filter(): Partial<TFilter> {
		return this._filter;
	}
	set filter(val: Partial<TFilter>) {
		val = omitBy(val, isNil) as Partial<TFilter>;
		if (!isEqual(val, omitBy(this.filter, isNil))) {
			this._filter = val;
			this._filter$.next(this._filter);
		}
	}

	// Displayed columns. Override this property if you need conditional hiding of some columns
	get displayedColumns(): string[] {
		return this.defaultDisplayedColumns;
	}

	readonly pageSizeOptions: number[];
	pageSize: number;

	get dataSource(): PaginatedDataSource<TListItemDto, TFilter> {
		return this._dataSource;
	}
	private _dataSource: PaginatedDataSource<TListItemDto, TFilter>;

	protected subscriptions = new Subscription();

	private _filter$: Subject<Partial<TFilter>>;
	// This is used for requested page (from URL) only, bind to the value from the response in the UI
	private _pageIndex: number = 0;
	private _requests = new EventEmitter<TFilter>();
	private flagSortingWorkaround = false;

	constructor(
		protected router: Router,
		protected activatedRoute: ActivatedRoute,
		/* The config service for the default page size, etc. */
		private configService: AppConfigService,
		/* The service for requesting the list from API */
		protected service: BaseApiService,
		/* Default visible columns */
		protected defaultDisplayedColumns: string[],
		/* Instance of the class for parsing filters from the Query String (doesn't need to include properties from `ListRequest`) */
		private _filter: Partial<TFilter>
	) {
		this._filter$ = new Subject<Partial<TFilter>>();
		this._dataSource = new PaginatedDataSource<TListItemDto, TFilter>(this.service, this._requests);
		this.pageSizeOptions = this.configService.getPageSizeOptions();
		this.pageSize = this.configService.getPageSize();
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
					// Make sure we modify the page/sort properties (and receive the data) after a processing delay to avoid
					// ExpressionChangedAfterItHasBeenCheckedError.
					// This seems like a poor design, but until we stop relying on the view children to get the sort properties
					// (the html defines the default sort,) we can't avoid it.
					delay(0)
				)
				.subscribe(params => {
					// Convert Query String parameters to TFilter instance
					let queryFilter = this.getFilterFromQueryString(params);
					console.debug('activatedRoute: ' + JSON.stringify(queryFilter));

					this._pageIndex = +(queryFilter?.pageIndex || 0);
					this.pageSize = +(queryFilter?.pageSize || this.configService.getPageSize());

					// Set grid Sorting
					if (!!this.sort && !!queryFilter?.orderBy) {
						// A workaround for a mat-table bug on showing sorting indication. See more https://stackoverflow.com/a/65501143/968003
						this.flagSortingWorkaround = true;
						this.sort.sort({ id: '', start: 'asc', disableClear: true });
						this.sort.sort({ id: queryFilter.orderBy, start: queryFilter.orderDirection || 'asc', disableClear: true });
						(this.sort.sortables.get(this.sort.active) as MatSortHeader)._setAnimationTransitionState({
							toState: 'active',
						});
						this.flagSortingWorkaround = false;
					}

					// Remove the common list properties from the rest of the filters
					[
						nameOf<ListRequest>('pageIndex'),
						nameOf<ListRequest>('pageSize'),
						nameOf<ListRequest>('orderBy'),
						nameOf<ListRequest>('orderDirection'),
					].forEach(prop => {
						delete queryFilter[prop];
					});
					// Set the sanitized filter for use in the custom filter bar of the list
					this.filter = queryFilter; // Duplicated values are filtered out downstream

					this.refreshList();
				})
		);
		// List of all triggers, which can cause refreshing data in the grid
		// Reset back to the first page if we change filters (anything, except the page number)
		const triggers = [
			this.paginator.page.pipe(
				tap((page: PageEvent) => {
					console.debug('TRIGGER: page event');
					if (!!page) {
						this._pageIndex = page.pageSize !== this.pageSize ? 0 : page.pageIndex;
						this.pageSize = page.pageSize;
						this.paginationHandler(page);
					}
				})
			),
			this.sort.sortChange.pipe(
				filter(() => !this.flagSortingWorkaround),
				tap(() => {
					console.debug('TRIGGER: sorting');
					this._pageIndex = 0;
				})
			),
			this._filter$.pipe(
				tap(f => {
					console.debug('TRIGGER: filter. ' + JSON.stringify(f));
					this._pageIndex = 0;
					if (!!get(f, 'search', null)) {
						// If we have changed the search filter then clear the sorting on the next request
						this.sort.sort({ id: '', start: 'asc', disableClear: false } as MatSortable);
					}
				})
			),
		];

		this.subscriptions.add(
			merge(...arrFilter(triggers, Boolean))
				.pipe(distinctUntilChanged(isEqual))
				.subscribe(() => {
					// Get merged filter from all the Query Parameters
					const accruedFilter = this.mergeFilters();
					console.debug('NAVIGATE: ' + JSON.stringify(accruedFilter));
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
		const paramsObj = orderBy(params.keys).reduce((obj, key) => {
			// 'params.getAll(key)' returns an array, when 'params.get(key)' - only the first value
			let value: string | string[] | { [x: string]: string } | null =
				this.filter[key as keyof TFilter] instanceof Array ? params.getAll(key) : params.get(key);
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
