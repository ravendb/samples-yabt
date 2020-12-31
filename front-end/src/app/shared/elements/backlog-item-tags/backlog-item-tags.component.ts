import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
	selector: 'backlog-item-tags',
	styleUrls: ['./backlog-item-tags.component.scss'],
	templateUrl: './backlog-item-tags.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BacklogItemTagsComponent {
	@Input()
	tags: string[] | undefined = undefined;
}
