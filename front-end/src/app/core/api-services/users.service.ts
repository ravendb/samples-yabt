import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ListResponse } from '@core/api-models/common/ListResponse';
import { UserReference } from '@core/api-models/common/references';
import { UserAddUpdRequest, UserGetByIdResponse } from '@core/api-models/user/item';
import { CurrentUserResponse } from '@core/api-models/user/item/CurrentUserResponse';
import { UserListGetRequest, UserListGetResponse } from '@core/api-models/user/list';
import { AppConfigService } from '@core/app-config.service';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { BaseApiService } from './base-api.service';

@Injectable({
	providedIn: 'root',
})
export class UsersService extends BaseApiService {
	private cachedCurrentUser: CurrentUserResponse | undefined;

	constructor(httpClient: HttpClient, appCfgService: AppConfigService) {
		super(httpClient, appCfgService.getAppServerUrl(), 'api/Users');
	}

	getUserList(request?: Partial<UserListGetRequest>): Observable<ListResponse<UserListGetResponse>> {
		return this.getList('', request);
	}
	getUser(id: string): Observable<UserGetByIdResponse> {
		return this.getItem(id);
	}
	getCurrentUser(): Observable<CurrentUserResponse> {
		if (!!this.cachedCurrentUser) return of(this.cachedCurrentUser!);
		return this.getItem<never, CurrentUserResponse>('current').pipe(tap(u => (this.cachedCurrentUser = u)));
	}

	createUser(request?: UserAddUpdRequest): Observable<UserReference> {
		return this.post('', request);
	}
	updateUser(id: string, request?: UserAddUpdRequest): Observable<UserReference> {
		if (!this.cachedCurrentUser && id == this.cachedCurrentUser!.id) this.cachedCurrentUser = undefined;

		return this.put(id, request);
	}
	deleteUser(id: string): Observable<UserReference> {
		return this.delete(id);
	}
}
