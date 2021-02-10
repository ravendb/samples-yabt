import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
	template: `
		<h2 mat-dialog-title>{{ data.title }}</h2>
		<div mat-dialog-content [innerHTML]="data.html"></div>
		<div mat-dialog-actions>
			<button mat-raised-button color="primary" [mat-dialog-close]="true" cdkFocusInitial>
				{{ data.confirmationText || 'Yes' }}
			</button>
			<button mat-raised-button mat-dialog-close>{{ data.cancellationText || 'No' }}</button>
		</div>
	`,
})
export class ConfirmationDialogComponent {
	constructor(@Inject(MAT_DIALOG_DATA) public data: IConfirmationDialogParams) {}
}

export interface IConfirmationDialogParams {
	title: string;
	html: string;
	confirmationText: string;
	cancellationText: string;
}
