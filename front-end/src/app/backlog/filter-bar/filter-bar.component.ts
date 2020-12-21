import { KeyValue } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { BacklogItemListGetRequest } from '@core/models/backlog-item/list/BacklogItemListGetRequest';
import { BacklogItemType } from '@core/models/common/BacklogItemType';
import { isNil } from 'lodash-es';
import { FilterBarComponentBase } from './filter-bar-base.component';

@Component({
	selector: 'filter-bar',
	styleUrls: ['./filter-bar.component.scss'],
	templateUrl: './filter-bar.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FilterBarComponent extends FilterBarComponentBase<BacklogItemListGetRequest> {
	dropdownOptions = ['All', 'Modified by me', 'Mention me'];

	get selectedOption(): string {
		return this._filter?.modifiedByTheCurrentUserOnly == true
			? this.dropdownOptions[1]
			: this._filter?.mentionsOfTheCurrentUserOnly == true
			? this.dropdownOptions[2]
			: this.dropdownOptions[0];
	}

	setCurrentOption(optionIndex: number): void {
		if (isNil(this._filter)) return;

		switch (optionIndex) {
			case 1:
				this._filter = { modifiedByTheCurrentUserOnly: true };
				break;
			case 2:
				this._filter = { mentionsOfTheCurrentUserOnly: true };
				break;
			default:
				this._filter = {};
				break;
		}
		this.filterChange.emit(this._filter);
	}

	get typeText(): BacklogItemType | 'Type' {
		return isNil(this._filter?.type) || this._filter.type == 'unknown' ? 'Type' : BacklogItemType[this._filter.type];
	}
	setType(value: keyof typeof BacklogItemType | unknown): void {
		let typedValue: keyof typeof BacklogItemType = !!value ? (value as keyof typeof BacklogItemType) : 'unknown';
		this._filter = typedValue !== 'unknown' ? { type: typedValue } : {};
		this.filterChange.emit(this._filter);
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
