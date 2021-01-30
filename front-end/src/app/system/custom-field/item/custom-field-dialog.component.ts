import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { BacklogItemType } from '@core/api-models/common/BacklogItemType';
import { CustomFieldType } from '@core/api-models/common/CustomFieldType';
import { CustomFieldAddRequest } from '@core/api-models/custom-field/item/CustomFieldAddRequest';
import { CustomFieldUpdateRequest } from '@core/api-models/custom-field/item/CustomFieldUpdateRequest';
import { CustomFieldsService } from '@core/api-services/customfields.service';
import { IKeyValuePair } from '@shared/filters';
import { CustomValidators } from '@utils/custom-validators';
import { take } from 'rxjs/operators';
import { IDialogData } from './IDialogData';

@Component({
	styleUrls: ['./custom-field-dialog.component.scss'],
	templateUrl: './custom-field-dialog.component.html',
})
export class CustomFieldDialogComponent implements OnInit {
	fieldTypes: IKeyValuePair[] = Object.keys(CustomFieldType).map(key => {
		return { key, value: CustomFieldType[key as keyof typeof CustomFieldType] };
	});
	backlogItemTypes: IKeyValuePair[] = Object.keys(BacklogItemType).map(key => {
		return { key, value: BacklogItemType[key as keyof typeof BacklogItemType] };
	});

	form!: FormGroupTyped<CustomFieldAddRequest & CustomFieldUpdateRequest>;

	constructor(
		public dialogRef: MatDialogRef<CustomFieldDialogComponent>,
		@Inject(MAT_DIALOG_DATA) public data: IDialogData | undefined,
		private fb: FormBuilder,
		private apiService: CustomFieldsService
	) {}

	ngOnInit() {
		this.form = this.fb.group({
			name: [null, [CustomValidators.required('Name'), CustomValidators.pattern(new RegExp('^[a-zA-Z][a-zA-Z0-9- ]*$'))]],
			type: [null, CustomValidators.required('Field Type')],
			isMandatory: [false],
			backlogItemTypes: null,
		}) as FormGroupTyped<CustomFieldAddRequest & CustomFieldUpdateRequest>;

		if (!!this.data?.id) {
			this.apiService.getCustomField(this.data.id).subscribe(item => {
				this.form.reset(item);
				// Disable changing the 'Type'
				this.form.controls.type.disable();
			});
		}
	}

	save(): void {
		var saveCmd = !!this.data?.id
			? this.apiService.updateCustomField(this.data.id, this.form.value)
			: this.apiService.createCustomField(this.form.value);
		saveCmd.pipe(take(1)).subscribe(
			ref => this.dialogRef.close(true),
			err => {}
		);
	}
}
