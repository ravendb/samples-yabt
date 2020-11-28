import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

@Component({
	selector: 'backlog-list',
	styleUrls: ['./backlog-list.component.scss'],
	templateUrl: './backlog-list.component.html',
})
export class BacklogListComponent implements OnInit, OnDestroy {
	private subscriptions: Subscription = new Subscription();

	constructor() {}

	ngOnInit() {}

	ngOnDestroy() {
		this.subscriptions.unsubscribe();
	}
}
