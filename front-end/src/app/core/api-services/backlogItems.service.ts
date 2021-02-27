import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BacklogAddUpdAllFieldsRequest } from '@core/api-models/backlog-item/item/BacklogAddUpdAllFieldsRequest';
import { BacklogItemGetResponseAllFields } from '@core/api-models/backlog-item/item/BacklogItemGetResponseAllFields';
import { BugAddUpdRequest } from '@core/api-models/backlog-item/item/BugAddUpdRequest';
import { UserStoryAddUpdRequest } from '@core/api-models/backlog-item/item/UserStoryAddUpdRequest';
import {
	BacklogItemListGetRequest,
	BacklogItemListGetResponse,
	BacklogItemTagListGetRequest,
	BacklogItemTagListGetResponse,
} from '@core/api-models/backlog-item/list';
import { BacklogItemType } from '@core/api-models/common/BacklogItemType';
import { ListResponse } from '@core/api-models/common/ListResponse';
import { BacklogItemReference } from '@core/api-models/common/references';
import { AppConfig } from '@core/app.config';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';

@Injectable({
	providedIn: 'root',
})
export class BacklogItemsService extends BaseApiService {
	constructor(httpClient: HttpClient) {
		super(httpClient, AppConfig.AppServerUrl, 'api/BacklogItems');
	}

	getBacklogItemList(request?: Partial<BacklogItemListGetRequest>): Observable<ListResponse<BacklogItemListGetResponse>> {
		return this.getList<BacklogItemListGetRequest, BacklogItemListGetResponse>('', request);
	}
	getBacklogItem(id: string): Observable<BacklogItemGetResponseAllFields> {
		return this.getItem<void, BacklogItemGetResponseAllFields>(`${id}`);
	}

	getBacklogItemTagList(request?: Partial<BacklogItemTagListGetRequest>): Observable<BacklogItemTagListGetResponse[]> {
		return this.getArray<BacklogItemTagListGetRequest, BacklogItemTagListGetResponse>('tags', request);
	}

	getSaveMethodByType<T extends BacklogAddUpdAllFieldsRequest>(
		type: keyof typeof BacklogItemType,
		id: string | null | undefined
	): (r: T) => Observable<BacklogItemReference> {
		switch (type) {
			case 'bug':
				return (request: BugAddUpdRequest) => (!!id ? this.put(`${id}/bug`, request) : this.post('/bug', request));
			case 'userStory':
				return (request: UserStoryAddUpdRequest) => (!!id ? this.put(`${id}/story`, request) : this.post('/story', request));
			default:
				throw new Error('Unsupported backlog item type');
		}
	}

	deleteBacklogItem(id: string): Observable<BacklogItemReference> {
		return this.delete(id);
	}
}
