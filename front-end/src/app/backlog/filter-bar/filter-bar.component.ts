import { KeyValue } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { BacklogItemListGetRequest } from '@core/models/backlog-item/list/BacklogItemListGetRequest';
import { CurrentUserRelations } from '@core/models/backlog-item/list/CurrentUserRelations';
import { BacklogItemType } from '@core/models/common/BacklogItemType';
import { IKeyValuePair } from '@shared/elements/filter-dropdown-button';
import { isNil } from 'lodash-es';
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
	types: IKeyValuePair[] = Object.keys(BacklogItemType).map(key => {
		return { key, value: BacklogItemType[key as keyof typeof BacklogItemType] };
	});

	ngOnInit(): void {
		super.ngOnInit();

		const triggers = [
			this.formGroup.controls.search.valueChanges.pipe(distinctUntilChanged(), debounceTime(400)),
			this.formGroup.controls.type.valueChanges.pipe(distinctUntilChanged()),
		];

		this.subscription.add(
			merge(...triggers)
				.pipe(delay(0))
				.subscribe(_ => this.applyFilter())
		);
	}

	ngOnDestroy() {
		this.subscription.unsubscribe();
	}

	// A workaround to iterate through the enum values in HTML template
	get currentUserRelations(): typeof CurrentUserRelations {
		return CurrentUserRelations;
	}
	get currentUserRelation(): CurrentUserRelations {
		let key: keyof typeof CurrentUserRelations = !isNil(this.formGroup.controls.currentUserRelation?.value)
			? this.formGroup.controls.currentUserRelation.value
			: 'none';
		return CurrentUserRelations[key];
	}
	setModeOption(value: string): void {
		let key: keyof typeof CurrentUserRelations = !isNil(value) ? (value as keyof typeof CurrentUserRelations) : 'none';
		this.formGroup.patchValue({ currentUserRelation: key });
		this.applyFilter();
	}

	get typeText(): BacklogItemType | 'Type' {
		return isNil(this._filter?.type) ? 'Type' : BacklogItemType[this._filter.type];
	}
	setType(value: keyof typeof BacklogItemType | unknown): void {
		let key: keyof typeof BacklogItemType | undefined = !isNil(value) ? (value as keyof typeof BacklogItemType) : undefined;
		this.formGroup.patchValue({ type: key });
		this.applyFilter();
	}
	// A workaround to iterate through the enum values in HTML template
	get backlogItemType(): typeof BacklogItemType {
		return BacklogItemType;
	}
	// A workaround ro preserve the original sorting order of the enum. By default, it's sorted alphabetically
	originalEnumOrder(a: KeyValue<string, BacklogItemType>, b: KeyValue<string, BacklogItemType>): number {
		return 0;
	}
}
