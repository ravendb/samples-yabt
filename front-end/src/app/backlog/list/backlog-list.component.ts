import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BacklogItemListGetRequest } from '@core/api-models/backlog-item/list/BacklogItemListGetRequest';
import { BacklogItemListGetResponse } from '@core/api-models/backlog-item/list/BacklogItemListGetResponse';
import { BacklogItemsService } from '@core/api-services/backlogItems.service';
import { AppConfigService } from '@core/app-config.service';
import { ListBaseComponent } from '@core/base-list/list-base.component';

@Component({
	styleUrls: ['./backlog-list.component.scss'],
	templateUrl: './backlog-list.component.html',
})
export class BacklogListComponent extends ListBaseComponent<BacklogItemListGetResponse, BacklogItemListGetRequest> {
	private static readonly defaultFilter: Partial<BacklogItemListGetRequest> = {
		currentUserRelation: undefined,
		types: [],
		states: [],
		tags: [],
		search: undefined,
		assignedUserId: undefined,
	};

	constructor(router: Router, activatedRoute: ActivatedRoute, configService: AppConfigService, apiService: BacklogItemsService) {
		super(
			router,
			activatedRoute,
			configService,
			apiService,
			['number', 'title', 'assignee', 'state', 'created', 'updated'],
			BacklogListComponent.defaultFilter
		);
	}
}
