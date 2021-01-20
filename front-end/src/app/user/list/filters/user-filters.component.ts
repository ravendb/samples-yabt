import { Component } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { UserListGetRequest } from '@core/api-models/user/list';
import { FilterBarComponentBase } from '@shared/filters/filter-bar-base.component';

@Component({
	selector: 'user-filters',
	styleUrls: ['./user-filters.component.scss'],
	templateUrl: './user-filters.component.html',
})
export class UserFiltersComponent extends FilterBarComponentBase<UserListGetRequest> {
	constructor(fb: FormBuilder) {
		super(fb);
	}
}
