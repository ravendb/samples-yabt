import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ListResponse } from '@core/api-models/common/ListResponse';
import { UserReference } from '@core/api-models/common/references';
import { UserAddUpdRequest, UserGetByIdResponse } from '@core/api-models/user/item';
import { UserListGetRequest, UserListGetResponse } from '@core/api-models/user/list';
import { AppConfig } from '@core/app.config';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';

@Injectable({
	providedIn: 'root',
})
export class UsersService extends BaseApiService {
	constructor(httpClient: HttpClient) {
		super(httpClient, AppConfig.AppServerUrl, 'api/Users');
	}

	getUserList(request?: Partial<UserListGetRequest>): Observable<ListResponse<UserListGetResponse>> {
		return this.getList('', request);
	}
	getUser(id: string): Observable<UserGetByIdResponse> {
		return this.getItem(id);
	}

	createUser(request?: UserAddUpdRequest): Observable<UserReference> {
		return this.post('', request);
	}
	updateUser(id: string, request?: UserAddUpdRequest): Observable<UserReference> {
		return this.put(id, request);
	}
	deleteUser(id: string): Observable<UserReference> {
		return this.delete(id);
	}
}
