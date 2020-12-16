import { AfterViewInit, Directive, EventEmitter, OnDestroy, ViewChild } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort, MatSortable, SortDirection } from '@angular/material/sort';
import { ActivatedRoute, Router } from '@angular/router';
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

	// Displayed columns. Override this property if you need conditional hiding of some columns
	get displayedColumns(): string[] {
		return this.defaultDisplayedColumns;
	}
	get pageSizeOptions(): number[] {
		return AppConfig.PageSizeOptions;
	}
	pageSize: number = AppConfig.PageSize;

	dataSource: PaginatedDataSource<TListItemDto, TFilter>;

	protected subscriptions = new Subscription();

	private filterBase = new BehaviorSubject<TFilter | null>(null);
	private clearSort: boolean = false;
	// This is used for requested page (from URL) only, bind to the value from the response in the UI
	private pageIndex: number = 0;
	private requests = new EventEmitter<TFilter>();

	constructor(
		protected router: Router,
		protected activatedRoute: ActivatedRoute,
		protected service: BaseApiService,
		protected defaultDisplayedColumns: string[]
	) {
		this.dataSource = new PaginatedDataSource<TListItemDto, TFilter>(this.service, this.requests);
	}

	// Subscribe for filter triggers after the nested components get initialised.
	// Hence, do it in AfterViewInit, instead of ngOnInit.
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
					this.pageIndex = +(params.get(nameOf<ListRequest>('pageIndex')) || 0);
					this.pageSize = +(params.get(nameOf<ListRequest>('pageSize')) || AppConfig.PageSize);

					// Set grid Sorting
					if (!!this.sort) {
						if (params.has(nameOf<ListRequest>('orderBy'))) {
							this.sort.active = params.get(nameOf<ListRequest>('orderBy')) || '';
							this.sort.direction = (params.get(nameOf<ListRequest>('orderDirection')) || '') as SortDirection;
						}
					}

					this.refreshList();
				})
		);

		// List of all triggers, which can cause refreshing data in the grid
		// Reset back to the first page if we change filters (anything, except the page number)
		const triggers = [
			this.paginator.page.pipe(
				tap((page: PageEvent) => {
					if (!!page) {
						this.pageIndex = page.pageSize !== this.pageSize ? 0 : page.pageIndex;
						this.pageSize = page.pageSize;
						this.paginationHandler(page);
					}
				})
			),
			this.sort.sortChange.pipe(
				tap(() => {
					this.pageIndex = 0;
				})
			),
			this.filterBase.pipe(filter(Boolean)),
			//this.filterComponent ? this.filterComponent.filterChange.pipe(tap(() => (this.pageIndex = 0))) : undefined,
		];
		/*
		if (this.filterComponent) {
			// Clear out the sorting whenever we change the 'search' field so that results are displayed based on relevance
			const searchControl: AbstractControl = this.filterComponent.form.get('search');
			if (searchControl) {
				this.subscriptions.add(
					searchControl.valueChanges.pipe(distinctUntilChanged(), filter(Boolean)).subscribe(() => (this.clearSort = true))
				);
			});
			}
		}*/

		this.subscriptions.add(
			merge(...arrFilter(triggers, Boolean))
				.pipe(
					distinctUntilChanged(),
					// A nasty way to wait for all of the components to finish updating the filter before refreshing the page
					debounceTime(100)
				)
				.subscribe(() => {
					// If we have changed the search filter then clear the sorting on the next request
					if (this.clearSort) {
						this.clearSort = false;
						this.clearSorting();
					}
					// Get merged filter from all the Query Parameters
					const accruedFilter = this.mergeFilters(false);
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
		const userFilter = this.mergeFilters(true);
		// Update the Data Source
		this.requests.emit(userFilter);
	}

	protected subscribeToEntityCreationNotificationEvent(event: EventEmitter<void>): void {
		this.subscriptions.add(event.subscribe(() => this.refreshList()));
	}

	protected paginationHandler(page: PageEvent): void {
		// Implementation in 'document-list.component.ts'
	}

	// Add filtering for the list
	protected mergeFilters(includeFilterBase: boolean): TFilter {
		const accruedFilter: TFilter = Object.assign(
			{} as TFilter,
			//includeFilterBase ? this.FilterBase : {},
			//this.filterComponent ? this.filterComponent.filter : {},
			{
				pageIndex: this.pageIndex,
				pageSize: this.pageSize,
			},
			this.sort && this.sort.active ? { orderBy: this.sort.active, orderDirection: this.sort.direction } : {}
		) as TFilter;
		return accruedFilter;
		/*
		return Object.keys(accruedFilter).reduce<TFilter>((prev, curr) => {
			let value = { [curr]: accruedFilter[curr] };
			// Convert 'Custom Filters' to a dictionary (flatten out filters, so they can go to the query string)
			if (isPlainObject(accruedFilter[curr])) {
				value = Object.keys(
					omitBy(accruedFilter[curr], isNil)
				) .reduce(
					(p, c) =>
						Object.assign(p, {
							[`${curr}[${c}]`]: moment.isMoment(accruedFilter[curr][c])
								? accruedFilter[curr][c].format('YYYY-MM-DD')
								: accruedFilter[curr][c],
						}),
					{}
				);
			}
			return Object.assign(prev, value);
		}, {} as TFilter);*/
	}
}
