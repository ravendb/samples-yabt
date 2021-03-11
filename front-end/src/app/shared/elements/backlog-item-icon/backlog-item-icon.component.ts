import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { BacklogItemType } from '@core/api-models/common/backlog-item';

@Component({
	selector: 'backlog-item-icon',
	styleUrls: ['./backlog-item-icon.component.scss'],
	templateUrl: './backlog-item-icon.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BacklogItemIconComponent {
	@Input()
	type: keyof typeof BacklogItemType | undefined;

	public get backlogItemType(): typeof BacklogItemType {
		return BacklogItemType;
	}
}
