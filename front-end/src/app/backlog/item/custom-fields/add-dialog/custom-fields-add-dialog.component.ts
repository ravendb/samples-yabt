import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { BacklogCustomFieldAction } from '@core/api-models/backlog-item/item/BacklogCustomFieldAction';
import { BacklogItemType } from '@core/api-models/common/backlog-item';
import { ListActionType } from '@core/api-models/common/ListActionType';
import { CustomFieldListGetResponse } from '@core/api-models/custom-field/list';
import { CustomFieldsService } from '@core/api-services/customfields.service';
import { IKeyValuePair } from '@shared/filters';
import { CustomValidators } from '@utils/custom-validators';
import { map } from 'rxjs/operators';

export interface ICustomFieldsAddDialogParams {
	currentFieldIds: string[] | undefined;
	backlogItemType: keyof typeof BacklogItemType;
}

@Component({
	templateUrl: './custom-fields-add-dialog.component.html',
	styleUrls: ['./custom-fields-add-dialog.component.scss'],
})
export class CustomFieldsAddDialogComponent implements OnInit {
	form!: FormGroupTyped<BacklogCustomFieldAction>;

	public get fields(): Observable<IKeyValuePair[]> {
		return this.apiService
			.getCustomFieldList({ backlogItemType: this.dialogParams.backlogItemType })
			.pipe(
				map(i =>
					i.entries
						?.map((e: CustomFieldListGetResponse) => <IKeyValuePair>{ key: e.id, value: e.name })
						.filter(
							(e: IKeyValuePair) => !this.dialogParams.currentFieldIds || this.dialogParams.currentFieldIds.includes(e.key)
						)
				)
			);
	}

	constructor(
		@Inject(MAT_DIALOG_DATA) private dialogParams: ICustomFieldsAddDialogParams,
		private fb: FormBuilder,
		private apiService: CustomFieldsService,
		private dialogRef: MatDialogRef<CustomFieldsAddDialogComponent>
	) {}

	ngOnInit(): void {
		const add: keyof typeof ListActionType = 'add';
		this.form = this.fb.group({
			customFieldId: [null, [CustomValidators.required()]],
			value: [null, [CustomValidators.required()]],
			actionType: [add, [CustomValidators.required()]],
		}) as FormGroupTyped<BacklogCustomFieldAction>;
	}

	add(): void {
		this.dialogRef.close(this.form.value);
	}
	close(): void {
		this.dialogRef.close();
	}
}
