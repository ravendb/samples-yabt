import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UsersService } from '@core/api-services/users.service';
import { ListBaseComponent } from '@core/base-list/list-base.component';
import { UserListGetRequest, UserListGetResponse } from '@core/models/user/list';

@Component({
	styleUrls: ['./user-list.component.scss'],
	templateUrl: './user-list.component.html',
})
export class UserListComponent extends ListBaseComponent<UserListGetResponse, UserListGetRequest> {
	private static readonly defaultFilter: Partial<UserListGetRequest> = {
		search: undefined,
	};

	constructor(router: Router, activatedRoute: ActivatedRoute, apiService: UsersService) {
		super(router, activatedRoute, apiService, ['name', 'created'], UserListComponent.defaultFilter);
	}
}
