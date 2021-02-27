import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
	selector: 'backlog-comments-icon',
	templateUrl: './comments-icon.component.html',
	styleUrls: ['./comments-icon.component.scss'],
	changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CommentsIconComponent {
	@Input()
	count: number | undefined;
	@Input()
	small: boolean = false;
}
