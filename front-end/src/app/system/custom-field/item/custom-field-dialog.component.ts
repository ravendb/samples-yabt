import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { BacklogItemType } from '@core/api-models/common/backlog-item';
import { CustomFieldType } from '@core/api-models/common/CustomFieldType';
import { CustomFieldAddRequest } from '@core/api-models/custom-field/item/CustomFieldAddRequest';
import { CustomFieldItemResponse } from '@core/api-models/custom-field/item/CustomFieldItemResponse';
import { CustomFieldUpdateRequest } from '@core/api-models/custom-field/item/CustomFieldUpdateRequest';
import { CustomFieldsService } from '@core/api-services/customfields.service';
import { NotificationService } from '@core/notification/notification.service';
import { IKeyValuePair } from '@shared/filters';
import { CustomValidators } from '@utils/custom-validators';
import { of, Subscription } from 'rxjs';
import { filter, switchMap, take } from 'rxjs/operators';
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

	private subscriptions = new Subscription();
	private _customFieldDtoBeforeUpdate: CustomFieldItemResponse | undefined;

	constructor(
		public dialogRef: MatDialogRef<CustomFieldDialogComponent>,
		@Inject(MAT_DIALOG_DATA) public data: IDialogData | undefined,
		private fb: FormBuilder,
		private apiService: CustomFieldsService,
		private notifyService: NotificationService
	) {}

	ngOnInit() {
		this.form = this.fb.group({
			name: [null, [CustomValidators.required('Name'), CustomValidators.pattern(new RegExp('^[a-zA-Z][a-zA-Z0-9- ]*$'))]],
			fieldType: [null, CustomValidators.required('Field Type')],
			isMandatory: [false],
			backlogItemTypes: null,
			usedInBacklogItemsCount: [{ value: '', disabled: true }],
		}) as FormGroupTyped<CustomFieldAddRequest & CustomFieldUpdateRequest>;

		const initObs = !!this.data?.id ? this.apiService.getCustomField(this.data.id) : of({} as CustomFieldItemResponse);
		this.subscriptions.add(
			initObs.subscribe(item => {
				this._customFieldDtoBeforeUpdate = item;
				this.form.reset(item);
				// Disable changing the 'Type'
				if (!!this.data?.id) this.form.controls.fieldType.disable();
			})
		);
	}

	save(): void {
		var saveCmd = !!this.data?.id
			? this.apiService.updateCustomField(this.data.id, this.form.value)
			: this.apiService.createCustomField(this.form.value);
		saveCmd.pipe(take(1)).subscribe(
			ref => {
				this.notifyService.showNotification(`Custom Field '${ref.name}' saved`);
				this.dialogRef.close(true);
			},
			err => {
				this.notifyService.showError('Failed to save', 'Invalid form values.', err);
			}
		);
	}

	delete(): void {
		if (!this.data?.id) return;

		this.notifyService
			.showDeleteConfirmation('Delete Custom Field?', `Do you want delete '<b>${this._customFieldDtoBeforeUpdate?.name}</b>'?`)
			.pipe(
				filter(r => r),
				switchMap(() => this.apiService.deleteCustomField(this.data!.id)),
				take(1)
			)
			.subscribe(
				ref => {
					this.notifyService.showNotification(`Custom field '${ref.name}' deleted`);
					this.dialogRef.close(true);
				},
				err => {
					this.notifyService.showError('Failed to delete', 'Deleting custom field failed:', err);
				}
			);
	}
}
