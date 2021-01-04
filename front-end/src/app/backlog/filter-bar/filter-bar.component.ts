import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { BacklogItemListGetRequest } from '@core/models/backlog-item/list/BacklogItemListGetRequest';
import { CurrentUserRelations } from '@core/models/backlog-item/list/CurrentUserRelations';
import { BacklogItemType } from '@core/models/common/BacklogItemType';
import { IKeyValuePair } from '@shared/filters';
import { merge } from 'rxjs';
import { debounceTime, delay, distinctUntilChanged } from 'rxjs/operators';
import { FilterBarComponentBase } from './filter-bar-base.component';

@Component({
	selector: 'filter-bar',
	styleUrls: ['./filter-bar.component.scss'],
	templateUrl: './filter-bar.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FilterBarComponent extends FilterBarComponentBase<BacklogItemListGetRequest> implements OnInit, OnDestroy {
	modes: IKeyValuePair[] = Object.keys(CurrentUserRelations).map(key => {
		return { key, value: CurrentUserRelations[key as keyof typeof CurrentUserRelations] };
	});
	types: IKeyValuePair[] = Object.keys(BacklogItemType).map(key => {
		return { key, value: BacklogItemType[key as keyof typeof BacklogItemType] };
	});

	ngOnInit(): void {
		super.ngOnInit();

		const triggers = [
			this.formGroup.controls.currentUserRelation.valueChanges.pipe(distinctUntilChanged()),
			this.formGroup.controls.search.valueChanges.pipe(distinctUntilChanged(), debounceTime(400)),
			this.formGroup.controls.type.valueChanges.pipe(distinctUntilChanged()),
		];

		this.subscription.add(
			merge(...triggers)
				.pipe(delay(0))
				.subscribe(() => this.applyFilter())
		);
	}

	ngOnDestroy() {
		this.subscription.unsubscribe();
	}
}
