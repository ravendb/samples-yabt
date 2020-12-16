import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ListBaseComponent } from '@core/base-list/list-base.component';
import { BacklogItemListGetRequest } from '@core/models/backlog-item/list/BacklogItemListGetRequest';
import { BacklogItemListGetResponse } from '@core/models/backlog-item/list/BacklogItemListGetResponse';
import { BacklogItemsService } from '@core/services/backlogItems.service';

@Component({
	selector: 'backlog-list',
	styleUrls: ['./backlog-list.component.scss'],
	templateUrl: './backlog-list.component.html',
})
export class BacklogListComponent
	extends ListBaseComponent<BacklogItemListGetResponse, BacklogItemListGetRequest>
	implements OnInit, OnDestroy {
	//	private subscriptions: Subscription = new Subscription();

	constructor(router: Router, activatedRoute: ActivatedRoute, apiService: BacklogItemsService) {
		super(router, activatedRoute, apiService, ['number', 'title', 'assignee', 'state', 'tags', 'created', 'updated']);
	}

	ngOnInit() {}

	ngOnDestroy() {
		//	this.subscriptions.unsubscribe();
	}
}
