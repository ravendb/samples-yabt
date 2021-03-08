import { CollectionViewer, DataSource } from '@angular/cdk/collections';
import { ListResponse } from '@core/api-models/common/ListResponse';
import { BaseApiService } from '@core/api-services/base-api.service';
import { isNil, omitBy } from 'lodash-es';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { catchError, delay, finalize, map, switchMap, tap } from 'rxjs/operators';

export class PaginatedDataSource<TListItemDto, TFilter> extends DataSource<TListItemDto> {
	private _totalRecords = new BehaviorSubject<number>(0);
	private _pageIndex = new BehaviorSubject<number>(0);
	private _loading = new BehaviorSubject<boolean>(false);

	totalRecords$ = this._totalRecords.asObservable();
	pageIndex$ = this._pageIndex.asObservable();
	loading$ = this._loading.asObservable();

	constructor(
		// HTTP Service for getting a list of items
		private service: BaseApiService,
		// Observable of events with new filter conditions
		private requests: Observable<TFilter>
	) {
		super();
	}

	connect(collectionViewer: CollectionViewer): Observable<TListItemDto[]> {
		return this.requests.pipe(
			// Turn on 'loading'
			tap(() => this._loading.next(true)),
			// Send request to the server
			switchMap((request: TFilter) =>
				this.service.getList<any, TListItemDto>('', omitBy(request, isNil)).pipe(
					// In case of an error show an empty list
					catchError(() => of(new ListResponse<TListItemDto>())),
					// Turn off 'loading' (important to use a nested pipe as the parent one doesn't complete)
					finalize(() => this._loading.next(false))
				)
			),
			// workaround for 'ExpressionChangedAfterItHasBeenCheckedError' in tests
			delay(0),
			// Return new entries
			map((response: ListResponse<TListItemDto>) => {
				this._totalRecords.next(response?.totalRecords || 0);
				this._pageIndex.next(response?.pageIndex || 0);
				return response?.entries || [];
			})
		);
	}

	disconnect(collectionViewer: CollectionViewer): void {
		this._loading.complete();
		this._pageIndex.complete();
		this._totalRecords.complete();
	}
}
