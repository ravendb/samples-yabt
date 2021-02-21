import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { ControlContainer, NG_VALUE_ACCESSOR } from '@angular/forms';
import { BacklogItemCommentListGetResponse } from '@core/api-models/backlog-item/item/BacklogItemCommentListGetResponse';
import { UserReference } from '@core/api-models/common/references';
import { CustomFormControlBase } from '@shared/forms/custom-form-control-base';
import { Subscription } from 'rxjs';

@Component({
	selector: 'backlog-item-comment',
	styleUrls: ['./backlog-item-comment.component.scss'],
	templateUrl: './backlog-item-comment.component.html',
	providers: [
		{
			provide: NG_VALUE_ACCESSOR,
			useExisting: BacklogItemCommentComponent,
			multi: true,
		},
	],
})
export class BacklogItemCommentComponent extends CustomFormControlBase<BacklogItemCommentListGetResponse> implements OnInit, OnDestroy {
	@Input()
	value: BacklogItemCommentListGetResponse | undefined;

	get author(): UserReference | undefined {
		return this.value?.author;
	}
	get content(): string | undefined {
		return this.value?.message;
	}
	get created(): Date | undefined {
		return this.value?.created;
	}

	private _subscriptions: Subscription = new Subscription();

	constructor(controlContainer: ControlContainer) {
		super(controlContainer);
	}

	ngOnInit() {}
	ngOnDestroy() {
		this._subscriptions.unsubscribe();
	}
}
