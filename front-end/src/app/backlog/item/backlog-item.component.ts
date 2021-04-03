import { Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { BacklogAddUpdAllFieldsRequest } from '@core/api-models/backlog-item/item/BacklogAddUpdAllFieldsRequest';
import { BacklogItemCommentListGetResponse } from '@core/api-models/backlog-item/item/BacklogItemCommentListGetResponse';
import { BacklogItemGetResponseAllFields } from '@core/api-models/backlog-item/item/BacklogItemGetResponseAllFields';
import { BacklogItemState, BacklogItemType, BugPriority, BugSeverity } from '@core/api-models/common/backlog-item';
import { UserReference } from '@core/api-models/common/references';
import { CurrentUserResponse } from '@core/api-models/user/item/CurrentUserResponse';
import { UserListGetRequest } from '@core/api-models/user/list';
import { BacklogItemsService } from '@core/api-services/backlogItems.service';
import { UsersService } from '@core/api-services/users.service';
import { INotificationMessage } from '@core/notification';
import { NotificationService } from '@core/notification/notification.service';
import { IBreadcrumbItem, PageTitleService } from '@core/page-title.service';
import { IKeyValuePair } from '@shared/filters';
import { CustomValidators } from '@utils/custom-validators';
import { Observable, of, Subscription } from 'rxjs';
import { filter, map, switchMap, take, tap } from 'rxjs/operators';
import { BacklogItemReadonlyProperties } from './backlog-item-readonly-properties';
import { BacklogItemFullHistoryDialogComponent } from './ui-elements/full-history-dialog/full-history-dialog.component';

@Component({
	templateUrl: './backlog-item.component.html',
	styleUrls: ['./backlog-item.component.scss'],
})
export class BacklogItemComponent implements OnInit {
	editId: string | null = null;
	form!: FormGroupTyped<BacklogAddUpdAllFieldsRequest>;
	dtoBeforeUpdate: BacklogItemReadonlyProperties | undefined;

	get loading$(): Observable<boolean> {
		return this.backlogService.loading$;
	}
	readonly states: IKeyValuePair[] = Object.keys(BacklogItemState).map(key => {
		return { key, value: BacklogItemState[key as keyof typeof BacklogItemState] };
	});
	readonly bugPriorities: IKeyValuePair[] = Object.keys(BugPriority).map(key => {
		return { key, value: BugPriority[key as keyof typeof BugPriority] };
	});
	readonly bugSeverities: IKeyValuePair[] = Object.keys(BugSeverity).map(key => {
		return { key, value: BugSeverity[key as keyof typeof BugSeverity] };
	});

	get typeTitle(): BacklogItemType | undefined {
		return !!this.dtoBeforeUpdate ? BacklogItemType[this.dtoBeforeUpdate.type] : undefined;
	}
	get type(): keyof typeof BacklogItemType | undefined {
		return this.dtoBeforeUpdate?.type;
	}

	private _currentUser: CurrentUserResponse | undefined;
	currentUserId$ = this.userService.getCurrentUser().pipe(
		tap(c => (this._currentUser = c)),
		map(c => c.id)
	);

	readonly searchByAssignee = (search: string): Observable<IKeyValuePair[]> =>
		this.userService
			.getUserList(<Partial<UserListGetRequest>>{ search, pageSize: 1000 })
			.pipe(map(r => r.entries?.map(t => <IKeyValuePair>{ key: t.id, value: t.nameWithInitials })));

	private subscriptions = new Subscription();
	private _listRoute = '/backlog-items';

	constructor(
		private activatedRoute: ActivatedRoute,
		private router: Router,
		private fb: FormBuilder,
		private dialog: MatDialog,
		private backlogService: BacklogItemsService,
		private userService: UsersService,
		private notifyService: NotificationService,
		private pageTitle: PageTitleService,
		private location: Location
	) {}

	ngOnInit(): void {
		this.form = this.fb.group({
			title: [null, [CustomValidators.required()]],
			state: [null, [CustomValidators.required()]],
			estimatedSize: [null],
			assigneeId: [null],
			tags: [null],
			changedRelatedItems: [null],
			changedCustomFields: [null],
			acceptanceCriteria: [null, [CustomValidators.requiredWhen(() => this.type == 'userStory')]],
			description: [null, [CustomValidators.requiredWhen(() => this.type == 'task' || this.type == 'feature')]],
			stepsToReproduce: [null, [CustomValidators.requiredWhen(() => this.type == 'bug')]],
			priority: [null],
			severity: [null],
		}) as FormGroupTyped<BacklogAddUpdAllFieldsRequest>;

		this.subscriptions.add(
			this.activatedRoute.paramMap
				.pipe(
					switchMap((p: ParamMap) => {
						const id = p.get('id');
						this.editId = !!id && id !== 'create' ? id : null;
						return !!this.editId ? this.backlogService.getBacklogItem(this.editId) : of({} as BacklogItemGetResponseAllFields);
					}),
					map(item => {
						// Save readonly fields
						this.dtoBeforeUpdate = item;
						// Convert to DTO for creating/editing
						return this.convertGetDtoToAddUpdDto(item);
					})
				)
				.subscribe(
					item => {
						this.form.reset(item);
						const lastBreadcrumbs: IBreadcrumbItem = {
							label: !!this.editId ? `#${this.editId}` : 'Create',
							url: '',
						};
						this.pageTitle.addLastBreadcrumbs(lastBreadcrumbs);
					},
					err => {
						this.notifyService.showError('Failed to fetch', '', err);
					}
				)
		);
	}
	ngOnDestroy() {
		this.subscriptions.unsubscribe();
	}

	save(): void {
		if (!this.type) return;

		const saveCmd = this.backlogService.saveMethodByType(this.type!, this.editId);
		saveCmd(this.form.value).subscribe(
			ref => {
				const notification = {
					linkRoute: [this._listRoute, ref.id],
					linkText: ref.name,
					text: 'Backlog item saved:',
				} as INotificationMessage;
				this.notifyService.showNotificationWithLink(notification);
			},
			err => {
				this.notifyService.showError('Failed to save', 'Invalid form values.', err);
			}
		);
	}

	delete(): void {
		if (!this.editId) return;

		this.notifyService
			.showDeleteConfirmation('Delete?', `Do you want delete '<b>${this.form?.controls.title.value}</b>'?`)
			.pipe(
				filter(r => r),
				switchMap(() => this.backlogService.deleteBacklogItem(this.editId!))
			)
			.subscribe(
				ref => {
					this.notifyService.showNotification(`Backlog item '${ref.name}' deleted`);
					this.goBack();
				},
				err => {
					this.notifyService.showError('Failed to delete', '', err);
				}
			);
	}

	removedComment(commentId: string): void {
		this.dtoBeforeUpdate!.comments = this.dtoBeforeUpdate!.comments.filter(c => c.id != commentId);
	}
	updatedComment(event: { id: string; message: string }): void {
		const comment = this.dtoBeforeUpdate!.comments.find(c => c.id == event.id);
		if (!!comment) {
			comment.message = event.message;
			comment.lastUpdated = new Date();
		}
	}
	addedComment(event: { id: string; message: string }): void {
		this.dtoBeforeUpdate!.comments.unshift({
			id: event.id,
			message: event.message,
			author: {
				id: this._currentUser?.id,
				name: this._currentUser?.nameWithInitials,
				fullName: this._currentUser?.fullName,
			} as UserReference,
			created: new Date(),
			lastUpdated: new Date(),
		} as BacklogItemCommentListGetResponse);
	}

	openHistoryDialog(): void {
		this.dialog
			.open(BacklogItemFullHistoryDialogComponent, { data: this.dtoBeforeUpdate?.historyDescOrder, minWidth: '350px' })
			.afterClosed()
			.pipe(take(1))
			.subscribe();
	}

	private goBack(): void {
		if (window.history.length > 1) this.location.back();
		else this.router.navigate([this._listRoute]);
	}

	private convertGetDtoToAddUpdDto(getDto: BacklogItemGetResponseAllFields): BacklogAddUpdAllFieldsRequest {
		if (!getDto) return {} as BacklogAddUpdAllFieldsRequest;

		return {
			title: getDto.title,
			state: getDto.state,
			estimatedSize: getDto.estimatedSize,
			assigneeId: getDto.assignee?.id,
			tags: getDto.tags,
			acceptanceCriteria: getDto.acceptanceCriteria,
			description: getDto.description,
			stepsToReproduce: getDto.stepsToReproduce,
		} as BacklogAddUpdAllFieldsRequest;
	}
}
