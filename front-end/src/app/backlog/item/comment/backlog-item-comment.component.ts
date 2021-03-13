import { formatDate } from '@angular/common';
import { Component, EventEmitter, Inject, Input, LOCALE_ID, Output } from '@angular/core';
import { BacklogItemCommentListGetResponse } from '@core/api-models/backlog-item/item/BacklogItemCommentListGetResponse';
import { UserReference } from '@core/api-models/common/references';
import { BacklogItemsService } from '@core/api-services/backlogItems.service';
import { NotificationService } from '@core/notification/notification.service';
import { filter, switchMap } from 'rxjs/operators';

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
	commentDeleted: EventEmitter<string> = new EventEmitter();
	@Output()
	commentSaved: EventEmitter<{ id: string; message: string }> = new EventEmitter();

	private _editingMode = false;
	get editingMode(): boolean {
		return this._editingMode || this.newComment;
	}
	editableContent: string = '';

	get author(): UserReference | undefined {
		return this.value?.author;
	}
	get content(): string | undefined {
		return this.value?.message || '';
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
		if (this.editingMode) {
			this.value!.created = new Date();
			this.value!.message = this.editableContent;
		} else {
			// Allowed to edit/delete own comments only
			if (this.author?.id != this.currentUserId) {
				this.notifyService.showError(`You're not the author`, 'Editing comments of other users is prohibited!');
				return;
			}

			this.editableContent = `${this.content}`;
		}
		this._editingMode = !this._editingMode;
	}

	delete(): void {
		if (!this.backlogItemId || !this.value?.id) return;

		this.notifyService
			.showDeleteConfirmation('Delete?', `Do you want delete comment of '<b>${this.getCurrentCommentDate()}</b>'?`)
			.pipe(
				filter(r => r),
				switchMap(() => this.backlogService.deleteComment(this.backlogItemId!, this.value!.id))
			)
			.subscribe(
				ref => {
					this.notifyService.showNotification(`Comment '${ref.name}' deleted`);
					this.commentDeleted.emit(this.value!.id);
				},
				err => {
					this.notifyService.showError('Failed to delete', '', err);
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
				this.commentSaved.emit({ id: ref.commentId!, message: this.editableContent });
				this.notifyService.showNotification(`Comment '${ref.name}' saved`);
				if (this.newComment) this.editableContent = '';
				else this.switchMode();
			},
			err => {
				this.notifyService.showError('Failed to save', 'Invalid form values.', err);
			}
		);
	}

	private getCurrentCommentDate(): string {
		return formatDate(this.created!, 'medium', this.locale);
	}
}
