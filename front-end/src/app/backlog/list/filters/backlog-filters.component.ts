import { Component, Input } from '@angular/core';
import { BacklogItemsService } from '@core/api-services/backlogItems.service';
import { UsersService } from '@core/api-services/users.service';
import { BacklogItemListGetRequest } from '@core/models/backlog-item/list/BacklogItemListGetRequest';
import { CurrentUserRelations } from '@core/models/backlog-item/list/CurrentUserRelations';
import { BacklogItemState } from '@core/models/common/BacklogItemState';
import { BacklogItemType } from '@core/models/common/BacklogItemType';
import { UserListGetRequest } from '@core/models/user/list';
import { IKeyValuePair } from '@shared/filters';
import { map } from 'rxjs/operators';

@Component({
	selector: 'backlog-filters',
	styleUrls: ['./backlog-filters.component.scss'],
	templateUrl: './backlog-filters.component.html',
})
export class BacklogFiltersComponent {
	modes: IKeyValuePair[] = Object.keys(CurrentUserRelations).map(key => {
		return { key, value: CurrentUserRelations[key as keyof typeof CurrentUserRelations] };
	});
	types: IKeyValuePair[] = Object.keys(BacklogItemType).map(key => {
		return { key, value: BacklogItemType[key as keyof typeof BacklogItemType] };
	});
	states: IKeyValuePair[] = Object.keys(BacklogItemState).map(key => {
		return { key, value: BacklogItemState[key as keyof typeof BacklogItemState] };
	});

	@Input()
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

	constructor(private backlogService: BacklogItemsService, private userService: UsersService) {}
}
