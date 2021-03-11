import { Component, Input } from '@angular/core';
import { BacklogItemHistoryRecord } from '@core/api-models/common/backlog-item';
import { ChangedByUserReference } from '@core/api-models/common/references';

@Component({
	selector: 'backlog-item-history-item',
	styleUrls: ['./backlog-item-history-item.component.scss'],
	templateUrl: './backlog-item-history-item.component.html',
})
export class BacklogItemHistoryItemComponent {
	@Input()
	item: ChangedByUserReference | undefined;

	@Input()
	isVerticalList = true;

	@Input()
	showSummary = false;

	get summary(): string | undefined {
		const i = this.item as BacklogItemHistoryRecord;
		return this.showSummary && !!i?.summary ? i.summary : '';
	}
}
