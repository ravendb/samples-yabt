import { ChangeDetectionStrategy, Component } from '@angular/core';
import { BacklogItemListGetRequest } from '@core/models/backlog-item/list/BacklogItemListGetRequest';
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
}
