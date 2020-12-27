import { KeyValue } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { BacklogItemListGetRequest } from '@core/models/backlog-item/list/BacklogItemListGetRequest';
import { CurrentUserRelations } from '@core/models/backlog-item/list/CurrentUserRelations';
import { BacklogItemType } from '@core/models/common/BacklogItemType';
import { isNil } from 'lodash-es';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { FilterBarComponentBase } from './filter-bar-base.component';

@Component({
	selector: 'filter-bar',
	styleUrls: ['./filter-bar.component.scss'],
	templateUrl: './filter-bar.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FilterBarComponent extends FilterBarComponentBase<BacklogItemListGetRequest> implements OnInit, OnDestroy {
	ngOnInit(): void {
		super.ngOnInit();

		this.subscription.add(
			this.formGroup.controls.search.valueChanges.pipe(debounceTime(400), distinctUntilChanged()).subscribe(_ => this.applyFilter())
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
		return isNil(this._filter?.type) || this._filter.type == 'unknown' ? 'Type' : BacklogItemType[this._filter.type];
	}
	setType(value: keyof typeof BacklogItemType | unknown): void {
		let key: keyof typeof BacklogItemType = !isNil(value) ? (value as keyof typeof BacklogItemType) : 'unknown';
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
