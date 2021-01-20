import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CustomFieldListGetRequest, CustomFieldListGetResponse } from '@core/api-models/custom-field/list';
import { CustomFieldsService } from '@core/api-services/customfields.service';
import { ListBaseComponent } from '@core/base-list/list-base.component';

@Component({
	styleUrls: ['./custom-field-list.component.scss'],
	templateUrl: './custom-field-list.component.html',
})
export class CustomFieldListComponent extends ListBaseComponent<CustomFieldListGetResponse, CustomFieldListGetRequest> {
	constructor(router: Router, activatedRoute: ActivatedRoute, apiService: CustomFieldsService) {
		super(router, activatedRoute, apiService, ['name', 'fieldType'], {});
	}
}
