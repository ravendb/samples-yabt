import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BacklogAddUpdAllFieldsRequest } from '@core/api-models/backlog-item/item/BacklogAddUpdAllFieldsRequest';
import { BacklogItemCommentAddUpdateRequest } from '@core/api-models/backlog-item/item/BacklogItemCommentAddUpdateRequest';
import { BacklogItemGetResponseAllFields } from '@core/api-models/backlog-item/item/BacklogItemGetResponseAllFields';
import { BugAddUpdRequest } from '@core/api-models/backlog-item/item/BugAddUpdRequest';
import { FeatureAddUpdRequest } from '@core/api-models/backlog-item/item/FeatureAddUpdRequest';
import { TaskAddUpdRequest } from '@core/api-models/backlog-item/item/TaskAddUpdRequest';
import { UserStoryAddUpdRequest } from '@core/api-models/backlog-item/item/UserStoryAddUpdRequest';
import {
	BacklogItemListGetRequest,
	BacklogItemListGetResponse,
	BacklogItemTagListGetRequest,
	BacklogItemTagListGetResponse,
} from '@core/api-models/backlog-item/list';
import { BacklogItemType } from '@core/api-models/common/backlog-item';
import { ListResponse } from '@core/api-models/common/ListResponse';
import { BacklogItemCommentReference, BacklogItemReference } from '@core/api-models/common/references';
import { AppConfigService } from '@core/app-config.service';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';

@Injectable({
	providedIn: 'root',
})
export class BacklogItemsService extends BaseApiService {
	constructor(httpClient: HttpClient, appCfgService: AppConfigService) {
		super(httpClient, appCfgService.getAppServerUrl(), 'api/BacklogItems');
	}

	getBacklogItemList(
		request?: Partial<BacklogItemListGetRequest>,
		setLoadingFlag: boolean = true
	): Observable<ListResponse<BacklogItemListGetResponse>> {
		return this.getList<BacklogItemListGetRequest, BacklogItemListGetResponse>('', request, setLoadingFlag);
	}
	getBacklogItem(id: string, setLoadingFlag: boolean = true): Observable<BacklogItemGetResponseAllFields> {
		return this.getItem<void, BacklogItemGetResponseAllFields>(`${id}`, undefined, setLoadingFlag);
	}

	getBacklogItemTagList(
		request?: Partial<BacklogItemTagListGetRequest>,
		setLoadingFlag: boolean = true
	): Observable<BacklogItemTagListGetResponse[]> {
		return this.getArray<BacklogItemTagListGetRequest, BacklogItemTagListGetResponse>('tags', request, setLoadingFlag);
	}

	saveMethodByType<T extends BacklogAddUpdAllFieldsRequest>(
		type: keyof typeof BacklogItemType,
		id: string | null | undefined
	): (r: T) => Observable<BacklogItemReference> {
		switch (type) {
			case 'bug':
				return (request: BugAddUpdRequest) => (!!id ? this.put(`${id}/bug`, request) : this.post('/bug', request));
			case 'userStory':
				return (request: UserStoryAddUpdRequest) => (!!id ? this.put(`${id}/story`, request) : this.post('/story', request));
			case 'task':
				return (request: TaskAddUpdRequest) => (!!id ? this.put(`${id}/task`, request) : this.post('/task', request));
			case 'feature':
				return (request: FeatureAddUpdRequest) => (!!id ? this.put(`${id}/feature`, request) : this.post('/feature', request));
			default:
				throw new Error('Unsupported backlog item type');
		}
	}

	deleteBacklogItem(id: string): Observable<BacklogItemReference> {
		return this.delete(id);
	}

	addComment(backlogItemId: string, message: string): Observable<BacklogItemCommentReference> {
		return this.post(`${backlogItemId}/comments`, new BacklogItemCommentAddUpdateRequest(message));
	}

	updateComment(backlogItemId: string, commentId: string, message: string): Observable<BacklogItemCommentReference> {
		return this.put(`${backlogItemId}/comments/${commentId}`, new BacklogItemCommentAddUpdateRequest(message));
	}

	deleteComment(backlogItemId: string, commentId: string): Observable<BacklogItemCommentReference> {
		return this.delete(`${backlogItemId}/comments/${commentId}`);
	}
}
