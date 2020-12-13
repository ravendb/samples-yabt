import { Component, Inject } from '@angular/core';
import { MAT_SNACK_BAR_DATA } from '@angular/material/snack-bar';

export interface INotificationMessage {
	text: string;
	linkText?: string;
	linkRoute?: string[];
	linkParams?: { [key: string]: string | string[] };
}

/*
    Snack notifications with HTML tags inside
*/
@Component({
	styles: [
		`
			a {
				color: lightblue;
				margin-left: 0.25em;
			}
		`,
	],
	template: `
		<div *ngFor="let data of messages">
			<span class="notification" [innerHTML]="data.text"></span>
			<a *ngIf="data.linkText" [routerLink]="data.linkRoute" [queryParams]="data.linkParams">{{ data.linkText }}</a>
		</div>
	`,
})
export class NotificationComponent {
	constructor(@Inject(MAT_SNACK_BAR_DATA) public messages: INotificationMessage[]) {}
}
