import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UserListGetRequest, UserListGetResponse } from '@core/api-models/user/list';
import { UsersService } from '@core/api-services/users.service';
import { AppConfigService } from '@core/app-config.service';
import { ListBaseComponent } from '@core/base-list/list-base.component';

@Component({
	styleUrls: ['./user-list.component.scss'],
	templateUrl: './user-list.component.html',
})
export class UserListComponent extends ListBaseComponent<UserListGetResponse, UserListGetRequest> {
	private static readonly defaultFilter: Partial<UserListGetRequest> = {
		search: undefined,
	};

	constructor(router: Router, activatedRoute: ActivatedRoute, configService: AppConfigService, apiService: UsersService) {
		super(router, activatedRoute, configService, apiService, ['name', 'created'], UserListComponent.defaultFilter);
	}
}
