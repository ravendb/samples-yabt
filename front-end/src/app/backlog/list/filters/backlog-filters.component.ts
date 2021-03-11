import { Component, Input, OnInit, Optional } from '@angular/core';
import { FormGroupDirective, FormGroupName } from '@angular/forms';
import { BacklogItemListGetRequest } from '@core/api-models/backlog-item/list/BacklogItemListGetRequest';
import { CurrentUserRelations } from '@core/api-models/backlog-item/list/CurrentUserRelations';
import { BacklogItemState, BacklogItemType } from '@core/api-models/common/backlog-item';
import { UserListGetRequest } from '@core/api-models/user/list';
import { BacklogItemsService } from '@core/api-services/backlogItems.service';
import { UsersService } from '@core/api-services/users.service';
import { IKeyValuePair } from '@shared/filters';
import { map } from 'rxjs/operators';

@Component({
	selector: 'backlog-filters',
	styleUrls: ['./backlog-filters.component.scss'],
	templateUrl: './backlog-filters.component.html',
})
export class BacklogFiltersComponent implements OnInit {
	modes: IKeyValuePair[] = Object.keys(CurrentUserRelations).map(key => {
		return { key, value: CurrentUserRelations[key as keyof typeof CurrentUserRelations] };
	});
	types: IKeyValuePair[] = Object.keys(BacklogItemType).map(key => {
		return { key, value: BacklogItemType[key as keyof typeof BacklogItemType] };
	});
	states: IKeyValuePair[] = Object.keys(BacklogItemState).map(key => {
		return { key, value: BacklogItemState[key as keyof typeof BacklogItemState] };
	});

	formGroup!: FormGroupTyped<BacklogItemListGetRequest>;

	@Input()
	isDialog: boolean = false;

	searchTagsByName = (search: string): Observable<IKeyValuePair[]> =>
		this.backlogService
			.getBacklogItemTagList({ search })
			.pipe(map(tags => tags.map(t => <IKeyValuePair>{ key: t.name, value: t.name })));

	searchByAssignee = (search: string): Observable<IKeyValuePair[]> =>
		this.userService
			.getUserList(<Partial<UserListGetRequest>>{ search, pageSize: 1000 })
			.pipe(map(r => r.entries?.map(t => <IKeyValuePair>{ key: t.id, value: t.nameWithInitials })));

	constructor(
		private formGroupDir: FormGroupDirective,
		@Optional() private formGroupName: FormGroupName,
		private backlogService: BacklogItemsService,
		private userService: UsersService
	) {}

	ngOnInit(): void {
		this.formGroup = (!!this.formGroupName?.name && this.formGroupName.name in this.formGroupDir.form.controls
			? this.formGroupDir.form.controls[this.formGroupName.name]
			: this.formGroupDir.form) as FormGroupTyped<BacklogItemListGetRequest>;
	}
}
