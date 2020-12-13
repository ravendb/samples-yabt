import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
	template: `
		<h2 mat-dialog-title>{{ data.title }}</h2>
		<mat-dialog-content [innerHTML]="data.html"></mat-dialog-content>
		<br />
		<mat-dialog-actions style="place-content: flex-end;">
			<button mat-raised-button mat-dialog-close color="primary">OK</button>
		</mat-dialog-actions>
	`,
})
export class AlertDialogComponent {
	constructor(@Inject(MAT_DIALOG_DATA) public data: any) {}
}
