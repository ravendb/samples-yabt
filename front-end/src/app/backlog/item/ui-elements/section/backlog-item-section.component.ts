import { Component, Input } from '@angular/core';

@Component({
	selector: 'backlog-item-section',
	styleUrls: ['./backlog-item-section.component.scss'],
	templateUrl: './backlog-item-section.component.html',
})
export class BacklogItemSectionComponent {
	@Input()
	title: string | undefined;

	@Input()
	collapsed: boolean = false;
}
