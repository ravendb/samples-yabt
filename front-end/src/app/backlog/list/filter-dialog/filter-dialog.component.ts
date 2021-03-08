import { AfterViewInit, Component, Inject } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { BacklogItemListGetRequest } from '@core/api-models/backlog-item/list/BacklogItemListGetRequest';
import { generateFormGroupFromObject } from '@utils/abstract-control';

@Component({
	selector: 'filter-dialog',
	styleUrls: ['./filter-dialog.component.scss'],
	templateUrl: './filter-dialog.component.html',
})
export class BacklogFilterDialogComponent implements AfterViewInit {
	formGroup: FormGroupTyped<BacklogItemListGetRequest>;

	constructor(
		@Inject(MAT_DIALOG_DATA) private initValue: Partial<BacklogItemListGetRequest>,
		private dialogRef: MatDialogRef<BacklogFilterDialogComponent>,
		fb: FormBuilder
	) {
		this.formGroup = generateFormGroupFromObject(fb, initValue);
	}

	ngAfterViewInit(): void {
		// Use AfterViewInit instead of OnInit, because some child components have reliance on too much
		this.formGroup.patchValue(this.initValue);
	}

	apply() {
		this.dialogRef.close(this.formGroup.value);
	}

	clear() {
		this.dialogRef.close();
	}
}
