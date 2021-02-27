import { Component, Input } from '@angular/core';
import { ChangedByUserReference } from '@core/api-models/common/references';

@Component({
	selector: 'backlog-item-history-item',
	styleUrls: ['./backlog-item-history-item.component.scss'],
	templateUrl: './backlog-item-history-item.component.html',
})
export class BacklogItemHistoryItemComponent {
	@Input()
	item: ChangedByUserReference | undefined;
}
