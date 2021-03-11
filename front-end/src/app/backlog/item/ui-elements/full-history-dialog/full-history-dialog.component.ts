import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { BacklogItemHistoryRecord } from '@core/api-models/common/backlog-item';

@Component({
	selector: 'app-full-history-dialog',
	templateUrl: './full-history-dialog.component.html',
	styleUrls: ['./full-history-dialog.component.scss'],
})
export class BacklogItemFullHistoryDialogComponent {
	constructor(
		@Inject(MAT_DIALOG_DATA) public history: BacklogItemHistoryRecord[],
		private dialogRef: MatDialogRef<BacklogItemFullHistoryDialogComponent>
	) {}

	close(): void {
		this.dialogRef.close();
	}
}
