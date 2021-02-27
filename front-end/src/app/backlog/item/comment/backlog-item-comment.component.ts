import { formatDate } from '@angular/common';
import { Component, EventEmitter, Inject, Input, LOCALE_ID, Output } from '@angular/core';
import { BacklogItemCommentListGetResponse } from '@core/api-models/backlog-item/item/BacklogItemCommentListGetResponse';
import { UserReference } from '@core/api-models/common/references';
import { BacklogItemsService } from '@core/api-services/backlogItems.service';
import { NotificationService } from '@core/notification/notification.service';
import { filter, switchMap, take } from 'rxjs/operators';

@Component({
	selector: 'backlog-item-comment',
	styleUrls: ['./backlog-item-comment.component.scss'],
	templateUrl: './backlog-item-comment.component.html',
})
export class BacklogItemCommentComponent {
	@Input()
	backlogItemId: string | null | undefined;
	@Input()
	value: BacklogItemCommentListGetResponse | undefined;
	@Input()
	newComment: boolean = false;
	@Input()
	currentUserId: string | undefined;

	@Output()
	deleted: EventEmitter<string> = new EventEmitter();

	editing = false;
	editableContent: string = '';

	get author(): UserReference | undefined {
		return this.value?.author;
	}
	get content(): string | undefined {
		return this.value?.message;
	}
	get created(): Date | undefined {
		return this.value?.created;
	}

	get loading$(): Observable<boolean> {
		return this.backlogService.loading$;
	}

	constructor(
		@Inject(LOCALE_ID) private locale: string,
		private backlogService: BacklogItemsService,
		private notifyService: NotificationService
	) {}

	switchMode(): void {
		if (this.newComment) {
			this.editing = true;
			return;
		}

		if (this.editing) {
			this.value!.created = new Date();
			this.value!.message = this.editableContent;
		} else {
			this.editableContent = `${this.content}`;
		}
		this.editing = !this.editing;
	}

	delete(): void {
		if (!this.backlogItemId || !this.value?.id) return;

		this.notifyService
			.showDeleteConfirmation('Delete?', `Do you want delete comment of '<b>${this.getCurrentCommentDate()}</b>'?`)
			.pipe(
				filter(r => r),
				switchMap(() => this.backlogService.deleteComment(this.backlogItemId!, this.value!.id)),
				take(1)
			)
			.subscribe(
				ref => {
					this.deleted.emit(this.value!.id);
					this.notifyService.showNotification(`Comment of '${this.getCurrentCommentDate()}' deleted`);
				},
				err => {
					this.notifyService.showError('Failed to delete', `Deleting failed: '${err}'`);
				}
			);
	}

	save(): void {
		if (!this.backlogItemId) return;

		const request = !this.value?.id
			? this.backlogService.addComment(this.backlogItemId!, this.editableContent)
			: this.backlogService.updateComment(this.backlogItemId!, this.value!.id!, this.editableContent);
		request.subscribe(
			ref => {
				const txt = ref.name.length > 7 ? ref.name.substring(0, 7) + '...' : ref.name;
				this.notifyService.showNotification(`Comment '${txt}' saved`);
			},
			err => {
				this.notifyService.showError('Failed to save', !!err?.detail ? err.detail : `Saving failed: '${err}'`);
			}
		);
	}

	private getCurrentCommentDate(): string {
		return formatDate(this.created!, 'medium', this.locale);
	}
}
