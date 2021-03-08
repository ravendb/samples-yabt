import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ListResponse } from '@core/api-models/common/ListResponse';
import { CustomFieldAddRequest } from '@core/api-models/custom-field/item/CustomFieldAddRequest';
import { CustomFieldItemResponse } from '@core/api-models/custom-field/item/CustomFieldItemResponse';
import { CustomFieldReferenceDto } from '@core/api-models/custom-field/item/CustomFieldReferenceDto';
import { CustomFieldUpdateRequest } from '@core/api-models/custom-field/item/CustomFieldUpdateRequest';
import { CustomFieldListGetRequest, CustomFieldListGetResponse } from '@core/api-models/custom-field/list';
import { AppConfigService } from '@core/app-config.service';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';

@Injectable({
	providedIn: 'root',
})
export class CustomFieldsService extends BaseApiService {
	constructor(httpClient: HttpClient, appCfgService: AppConfigService) {
		super(httpClient, appCfgService.getAppServerUrl(), 'api/CustomFields');
	}

	getCustomField(id: string): Observable<CustomFieldItemResponse> {
		return this.getItem(id);
	}

	getCustomFieldList(request?: Partial<CustomFieldListGetRequest>): Observable<ListResponse<CustomFieldListGetResponse>> {
		return this.getList('', request);
	}

	createCustomField(request?: CustomFieldAddRequest): Observable<CustomFieldReferenceDto> {
		return this.post('', request);
	}

	updateCustomField(id: string, request?: CustomFieldUpdateRequest): Observable<CustomFieldReferenceDto> {
		return this.put(id, request);
	}

	deleteCustomField(id: string): Observable<CustomFieldReferenceDto> {
		return this.delete(id);
	}
}
