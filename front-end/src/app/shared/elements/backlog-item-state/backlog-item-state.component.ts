import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { BacklogItemState } from '@core/api-models/common/backlog-item/BacklogItemState';

@Component({
	selector: 'backlog-item-state',
	styleUrls: ['./backlog-item-state.component.scss'],
	templateUrl: './backlog-item-state.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BacklogItemStateComponent {
	@Input()
	state: keyof typeof BacklogItemState = 'new';

	public get backlogItemState(): typeof BacklogItemState {
		return BacklogItemState;
	}
}
