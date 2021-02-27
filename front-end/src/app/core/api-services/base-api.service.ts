import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { ListResponse } from '@core/api-models/common/ListResponse';
import { get, isEmpty, trim, trimEnd } from 'lodash-es';
import { BehaviorSubject, Observable, of, throwError as observableThrowError } from 'rxjs';
import { catchError, finalize, switchMap, take, tap } from 'rxjs/operators';
import { HttpErrorDetails } from './http-error-details';
import { HttpParamForcedUriCodec } from './http-param-forced-url-codec';

export abstract class BaseApiService {
	private url: string;

	private _loading = new BehaviorSubject<boolean>(false);
	loading$ = this._loading.asObservable();

	constructor(protected httpClient: HttpClient, private apiBaseUrl: string, controllerUrl: string) {
		this.url = this.getBaseUrl(controllerUrl);
	}

	// Note: This method is public as it is used by 'paginated-datasource' but it should not be used anywhere else
	getList<TRequestDto, TListItemDto>(urlExtension?: string, dto?: Partial<TRequestDto>): Observable<ListResponse<TListItemDto>> {
		return this.wrappedRequest(() =>
			this.httpClient.get<ListResponse<TListItemDto>>(this.getFullUrl(urlExtension), {
				params: this.toHttpParams(dto),
			})
		);
	}

	// This method gets all the records without paging and filtering
	protected getAll<TListItemDto>(urlExtension?: string): Observable<TListItemDto[]> {
		return this.wrappedRequest(() => this.httpClient.get<TListItemDto[]>(this.getFullUrl(urlExtension)));
	}

	protected getItem<TRequestDto, TItemDto>(urlExtension?: string, dto?: Partial<TRequestDto>): Observable<TItemDto> {
		return this.wrappedRequest(() =>
			this.httpClient.get<TItemDto>(this.getFullUrl(urlExtension), {
				params: this.toHttpParams(dto),
			})
		);
	}

	protected getArray<TRequestDto, TListItemDto>(
		urlExtension?: string,
		dto?: Partial<TRequestDto> | { [param: string]: string | string[] }
	): Observable<TListItemDto[]> {
		return this.wrappedRequest(() =>
			this.httpClient.get<TListItemDto[]>(this.getFullUrl(urlExtension), {
				params: this.toHttpParams(dto),
			})
		);
	}

	protected patch<TItemDto, TResultDto>(id: string, replacements: Partial<TItemDto>): Observable<TResultDto> {
		const body = Object.keys(replacements).map(path => ({ op: 'replace', path, value: get(replacements, path) }));
		return this.wrappedRequest(() => this.httpClient.patch<TResultDto>(this.getFullUrl(id), body));
	}

	protected post<TItemDto, TResultDto>(urlExtension?: string, body?: TItemDto): Observable<TResultDto> {
		return this.wrappedRequest(() =>
			this.httpClient
				// always post with a content type of application/json to prevent Http Status 415 errors
				.post<TResultDto>(this.getFullUrl(urlExtension), body, {
					headers: { 'Content-Type': 'application/json' },
				})
		);
	}

	protected put<TItemDto, TResultDto>(urlExtension?: string, body?: TItemDto): Observable<TResultDto> {
		return this.wrappedRequest(() => this.httpClient.put<TResultDto>(this.getFullUrl(urlExtension), body));
	}

	protected delete<TResultDto>(urlExtension?: string): Observable<TResultDto> {
		return this.wrappedRequest(() => this.httpClient.delete<TResultDto>(this.getFullUrl(urlExtension)));
	}

	protected getFullUrl(urlExtension?: string): string {
		const lowCaseUrl = urlExtension?.toLowerCase() ?? '';
		// End-point on a different controller
		if (lowCaseUrl.startsWith('api') || lowCaseUrl.startsWith('/api')) return this.getBaseUrl(urlExtension!);

		return `${this.url}${isEmpty(urlExtension) ? '' : '/'}${trim(urlExtension, '/')}`;
	}

	protected toHttpParams(input: { [key: string]: any } | undefined): HttpParams {
		// Convert object to HttpParams (see https://stackoverflow.com/a/47928379/968003).
		// Use custom encoding to work around problems with encoding of '+'.
		return new HttpParams({
			encoder: new HttpParamForcedUriCodec(),
			fromObject: input,
		});
	}

	private wrappedRequest<T>(request: () => Observable<T>): Observable<T> {
		return of(null).pipe(
			tap(() => this._loading.next(true)),
			switchMap(_ => request()),
			take(1),
			catchError(e => this.handleErrorObservable(e)),
			finalize(() => this._loading.next(false))
		);
	}

	private handleErrorObservable(response: HttpErrorResponse): Observable<never> {
		return observableThrowError(response?.error || ({ detail: response?.message } as HttpErrorDetails)); // TODO: extend the propagated error message with title/description
	}

	private getBaseUrl(urlExtension: string): string {
		const lowCaseUrl = urlExtension.toLowerCase();
		return lowCaseUrl.startsWith('http') ? lowCaseUrl : `${trimEnd(this.apiBaseUrl, '/ ')}/${trim(lowCaseUrl, '/ ')}`;
	}
}
