import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ListResponse } from '@core/api-models/common/ListResponse';
import { CustomFieldListGetRequest, CustomFieldListGetResponse } from '@core/api-models/custom-field/list';
import { AppConfig } from '@core/app.config';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';

@Injectable({
	providedIn: 'root',
})
export class CustomFieldsService extends BaseApiService {
	constructor(httpClient: HttpClient) {
		super(httpClient, AppConfig.AppServerUrl, 'api/CustomFields');
	}

	getCustomFieldList(request?: Partial<CustomFieldListGetRequest>): Observable<ListResponse<CustomFieldListGetResponse>> {
		return this.getList<CustomFieldListGetRequest, CustomFieldListGetResponse>('', request);
	}
}
