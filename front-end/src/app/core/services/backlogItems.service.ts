import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AppConfig } from '@core/app.config';
import { BacklogItemGetResponseBase } from '@core/models/backlog-item/item/BacklogItemGetResponseBase';
import { BacklogItemListGetRequest } from '@core/models/backlog-item/list/BacklogItemListGetRequest';
import { BacklogItemListGetResponse } from '@core/models/backlog-item/list/BacklogItemListGetResponse';
import { ListResponse } from '@core/models/common/ListResponse';
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

	/*	updatePerson(id: string, dto: PersonContactCreateUpdateDto): Observable<any> {
		return this.put(`person/${id}`, TransformDatesOnObject(PersonContactCreateUpdateDto, dto));
	}

	updateCompany(id: string, dto: CompanyContactCreateUpdateDto): Observable<any> {
		return this.put(`company/${id}`, TransformDatesOnObject(CompanyContactCreateUpdateDto, dto));
	}

	createPerson(dto: PersonContactCreateUpdateDto): Observable<EntityReference> {
		return this.post<PersonContactCreateUpdateDto, EntityReference>(`person`, TransformDatesOnObject(PersonContactCreateUpdateDto, dto));
	}

	createCompany(dto: CompanyContactCreateUpdateDto): Observable<EntityReference> {
		return this.post<CompanyContactCreateUpdateDto, EntityReference>(`company`, TransformDatesOnObject(CompanyContactCreateUpdateDto, dto));
	}

	exportContactsByFilter(request?: Partial<ContactListRequest>): Observable<HttpResponse<Blob>> {
		return this.getBlob<ContactListRequest>(`exportByFilter`, request);
	}

	exportContactsByIds(request: IdCollectionRequest): Observable<HttpResponse<Blob>> {
		return this.getBlob<IdCollectionRequest>(`exportByIds`, request);
	}

	lookupContactList(term: string, type: keyof typeof ContactLookupType): Observable<ContactLookupDto[]> {
		return this.getArray<string, ContactLookupDto>(`lookup/${term}`, !!type ? { type: type.toString() } : null);
	}

	lookupContact(id: string): Observable<ContactLookupDto> {
		return this.getItem<void, ContactLookupDto>(`${id}/lookup`);
	}

	getContactList(request?: Partial<ContactListRequest>): Observable<ListResponse<ContactListItemDto>> {
		return this.getList<ContactListRequest, ContactListItemDto>('', request);
	}

	getContact(id: string): Observable<ContactViewDto> {
		return this.getItem<void, ContactViewDto>(`${id}`);
	}

	isPhoneValid(dto: PhoneCheckDto): Observable<boolean> {
		return this.getItem<PhoneCheckDto, boolean>(`IsPhoneValid`, dto);
	}

	isAbnValid(dto: RegNumberCheckDto): Observable<boolean> {
		return this.getItem<RegNumberCheckDto, boolean>(`IsAbnValid`, dto);
	}

	isAcnValid(dto: RegNumberCheckDto): Observable<boolean> {
		return this.getItem<RegNumberCheckDto, boolean>(`IsAcnValid`, dto);
	}

	addDocument(id: string, request: DocumentRequest): Observable<EntityReference> {
		return this.post<DocumentRequest, EntityReference>(`${id}/documents`, request);
	}
*/
}
