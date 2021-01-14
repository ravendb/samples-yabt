import { Component, Input } from '@angular/core';
import { BacklogItemListGetRequest } from '@core/models/backlog-item/list/BacklogItemListGetRequest';
import { CurrentUserRelations } from '@core/models/backlog-item/list/CurrentUserRelations';
import { BacklogItemState } from '@core/models/common/BacklogItemState';
import { BacklogItemType } from '@core/models/common/BacklogItemType';
import { IKeyValuePair } from '@shared/filters';

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
}
