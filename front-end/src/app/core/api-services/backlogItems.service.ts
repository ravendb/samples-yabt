import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BacklogItemGetResponseBase } from '@core/api-models/backlog-item/item/BacklogItemGetResponseBase';
import {
	BacklogItemListGetRequest,
	BacklogItemListGetResponse,
	BacklogItemTagListGetRequest,
	BacklogItemTagListGetResponse,
} from '@core/api-models/backlog-item/list';
import { ListResponse } from '@core/api-models/common/ListResponse';
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
	getBacklogItem(id: string): Observable<BacklogItemGetResponseBase> {
		return this.getItem<void, BacklogItemGetResponseBase>(`${id}`);
	}

	getBacklogItemTagList(request?: Partial<BacklogItemTagListGetRequest>): Observable<BacklogItemTagListGetResponse[]> {
		return this.getArray<BacklogItemTagListGetRequest, BacklogItemTagListGetResponse>('tags', request);
	}
}
