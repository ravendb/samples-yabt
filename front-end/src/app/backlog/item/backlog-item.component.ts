import { Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { BacklogAddUpdAllFieldsRequest } from '@core/api-models/backlog-item/item/BacklogAddUpdAllFieldsRequest';
import { BacklogItemGetResponseAllFields } from '@core/api-models/backlog-item/item/BacklogItemGetResponseAllFields';
import { BacklogItemState } from '@core/api-models/common/BacklogItemState';
import { BacklogItemType } from '@core/api-models/common/BacklogItemType';
import { BacklogRelationshipType } from '@core/api-models/common/BacklogRelationshipType';
import { BugPriority } from '@core/api-models/common/BugPriority';
import { BugSeverity } from '@core/api-models/common/BugSeverity';
import { UserListGetRequest } from '@core/api-models/user/list';
import { BacklogItemsService } from '@core/api-services/backlogItems.service';
import { UsersService } from '@core/api-services/users.service';
import { NotificationService } from '@core/notification/notification.service';
import { IBreadcrumbItem, PageTitleService } from '@core/page-title.service';
import { IKeyValuePair } from '@shared/filters';
import { CustomValidators } from '@utils/custom-validators';
import { of, Subscription } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { BacklogItemReadonlyProperties } from './backlog-item-readonly-properties';

@Component({
	templateUrl: './backlog-item.component.html',
	styleUrls: ['./backlog-item.component.scss'],
})
export class BacklogItemComponent implements OnInit {
	editId: string | null = null;
	form!: FormGroupTyped<BacklogAddUpdAllFieldsRequest>;
	dtoBeforeUpdate: BacklogItemReadonlyProperties | undefined;

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
			assigneeId: [null],
			tags: [null],
			relatedItems: [null],
			customFields: [null],
			acceptanceCriteria: [null, [CustomValidators.requiredWhen(() => this.type == 'userStory')]],
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
				.subscribe(item => {
					this.form.reset(item);
					const lastBreadcrumbs: IBreadcrumbItem = {
						label: !!this.editId ? `#${this.editId}` : 'Create',
						url: '',
					};
					this.pageTitle.addLastBreadcrumbs(lastBreadcrumbs);
				})
		);
	}
	ngOnDestroy() {
		this.subscriptions.unsubscribe();
	}

	save(): void {
		/*var saveCmd = !!this.editId
			? this.apiService.updateUser(this.editId, this.form.value)
			: this.apiService.createUser(this.form.value);
		saveCmd.pipe(take(1)).subscribe(
			ref => {
				const notification = {
					linkRoute: [this._listRoute, ref.id],
					linkText: ref.name,
					text: 'User saved:',
				} as INotificationMessage;
				this.notifyService.showNotificationWithLink(notification);
				this.goBack();
			},
			err => {
				this.notifyService.showError('Failed to save', `Saving custom field failed: '${err}'`);
			}
		);*/
	}

	delete(): void {
		if (!this.editId) return;

		/*this.notifyService
			.showDeleteConfirmation('Delete Custom Field?', `Do you want delete '<b>${this._userDtoBeforeUpdate?.nameWithInitials}</b>'?`)
			.pipe(
				filter(r => r),
				switchMap(() => this.apiService.deleteUser(this.editId!)),
				take(1)
			)
			.subscribe(
				ref => {
					this.notifyService.showNotification(`User '${ref.name}' deleted`);
					this.goBack();
				},
				err => {
					this.notifyService.showError('Failed to delete', `Deleting user failed: '${err}'`);
				}
			);*/
	}

	goBack(): void {
		if (window.history.length > 1) this.location.back();
		else this.router.navigate([this._listRoute]);
	}

	private convertGetDtoToAddUpdDto(getDto: BacklogItemGetResponseAllFields): BacklogAddUpdAllFieldsRequest {
		if (!getDto) return {} as BacklogAddUpdAllFieldsRequest;

		const related = getDto.relatedItems?.reduce((result, i) => {
			if (!!i.relatedTo?.id) result[i.relatedTo!.id!] = i.linkType;
			return result;
		}, {} as { [key: string]: BacklogRelationshipType });

		return {
			title: getDto.title,
			state: getDto.state,
			assigneeId: getDto.assignee?.id,
			tags: getDto.tags,
			relatedItems: related,
			customFields: getDto.customFields,
			acceptanceCriteria: getDto.acceptanceCriteria,
			stepsToReproduce: getDto.stepsToReproduce,
		} as BacklogAddUpdAllFieldsRequest;
	}
}
