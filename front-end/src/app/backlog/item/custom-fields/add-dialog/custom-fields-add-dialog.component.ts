import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormControl } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { BacklogCustomFieldAction } from '@core/api-models/backlog-item/item/BacklogCustomFieldAction';
import { ListActionType } from '@core/api-models/common/ListActionType';
import { CustomFieldListGetResponse } from '@core/api-models/custom-field/list';
import { CustomValidators } from '@utils/custom-validators';
import { Subscription } from 'rxjs';

export interface ICustomFieldsAddDialogParams {
	availableFields: CustomFieldListGetResponse[] | undefined;
}

@Component({
	templateUrl: './custom-fields-add-dialog.component.html',
	styleUrls: ['./custom-fields-add-dialog.component.scss'],
})
export class CustomFieldsAddDialogComponent implements OnInit {
	form!: FormGroupTyped<BacklogCustomFieldAction>;
	customFieldCtrl = new FormControl(null, [CustomValidators.required()]) as FormControlTyped<CustomFieldListGetResponse>;

	private subscriptions = new Subscription();

	constructor(
		@Inject(MAT_DIALOG_DATA) public dialogParams: ICustomFieldsAddDialogParams,
		private fb: FormBuilder,
		private dialogRef: MatDialogRef<CustomFieldsAddDialogComponent>
	) {}

	ngOnInit(): void {
		const add: keyof typeof ListActionType = 'add';
		this.form = this.fb.group({
			customFieldId: [null, [CustomValidators.required()]],
			value: [null, [CustomValidators.required()]],
			actionType: [add, [CustomValidators.required()]],
		}) as FormGroupTyped<BacklogCustomFieldAction>;

		this.subscriptions.add(
			this.customFieldCtrl.valueChanges.subscribe(val => {
				this.form.controls.customFieldId.setValue(val.id);
			})
		);
	}

	add(): void {
		this.dialogRef.close(this.form.value);
	}
	close(): void {
		this.dialogRef.close();
	}
}
