import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { ListResponse } from '@core/models/common/ListResponse';
import { get, isEmpty, trim, trimEnd } from 'lodash-es';
import { Observable, throwError as observableThrowError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpParamForcedUriCodec } from './http-param-forced-url-codec';

export abstract class BaseApiService {
	private url: string;

	constructor(protected httpClient: HttpClient, private apiBaseUrl: string, controllerUrl: string) {
		this.url = this.getBaseUrl(controllerUrl);
	}

	// Note: This method is public as it is used by generic.data.source but it should not be used anywhere else
	getList<TRequestDto, TListItemDto>(urlExtension?: string, dto?: Partial<TRequestDto>): Observable<ListResponse<TListItemDto>> {
		return this.httpClient
			.get<ListResponse<TListItemDto>>(this.getFullUrl(urlExtension), {
				params: this.toHttpParams(dto),
			})
			.pipe(catchError(e => this.handleErrorObservable(e)));
	}

	// This method gets all the records without paging and filtering
	getAll<TListItemDto>(urlExtension?: string): Observable<TListItemDto[]> {
		return this.httpClient.get<TListItemDto[]>(this.getFullUrl(urlExtension)).pipe(catchError(e => this.handleErrorObservable(e)));
	}

	protected getItem<TRequestDto, TItemDto>(urlExtension?: string, dto?: Partial<TRequestDto>): Observable<TItemDto> {
		return this.httpClient
			.get<TItemDto>(this.getFullUrl(urlExtension), {
				params: this.toHttpParams(dto),
			})
			.pipe(catchError(e => this.handleErrorObservable(e)));
	}

	protected getArray<TRequestDto, TListItemDto>(
		urlExtension?: string,
		dto?: Partial<TRequestDto> | { [param: string]: string | string[] }
	): Observable<TListItemDto[]> {
		return this.httpClient
			.get<TListItemDto[]>(this.getFullUrl(urlExtension), {
				params: this.toHttpParams(dto),
			})
			.pipe(catchError(e => this.handleErrorObservable(e)));
	}

	protected patch<TItemDto, TResultDto>(id: string, replacements: Partial<TItemDto>): Observable<TResultDto> {
		const body = Object.keys(replacements).map(path => ({ op: 'replace', path, value: get(replacements, path) }));
		return this.httpClient.patch<TResultDto>(this.getFullUrl(id), body).pipe(catchError(e => this.handleErrorObservable(e)));
	}

	protected post<TItemDto, TResultDto>(urlExtension?: string, body?: TItemDto): Observable<TResultDto> {
		return (
			this.httpClient
				// always post with a content type of application/json to prevent Http Status 415 errors
				.post<TResultDto>(this.getFullUrl(urlExtension), body, {
					headers: { 'Content-Type': 'application/json' },
				})
				.pipe(catchError(e => this.handleErrorObservable(e)))
		);
	}

	protected put<TItemDto, TResultDto>(urlExtension?: string, body?: TItemDto): Observable<TResultDto> {
		return this.httpClient.put<TResultDto>(this.getFullUrl(urlExtension), body).pipe(catchError(e => this.handleErrorObservable(e)));
	}

	protected delete<TResultDto>(urlExtension?: string): Observable<TResultDto> {
		return this.httpClient.delete<TResultDto>(this.getFullUrl(urlExtension)).pipe(catchError(e => this.handleErrorObservable(e)));
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

	private handleErrorObservable(response: HttpErrorResponse) {
		return observableThrowError(response?.message); // TODO: extend the propagated error message with title/description
	}

	private getBaseUrl(urlExtension: string): string {
		const lowCaseUrl = urlExtension.toLowerCase();
		return lowCaseUrl.startsWith('http') ? lowCaseUrl : `${trimEnd(this.apiBaseUrl, '/ ')}/${trim(lowCaseUrl, '/ ')}`;
	}
}
