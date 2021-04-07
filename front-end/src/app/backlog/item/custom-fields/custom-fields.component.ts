import { Component, Input, Optional, Self } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { BacklogCustomFieldAction } from '@core/api-models/backlog-item/item/BacklogCustomFieldAction';
import { BacklogItemCustomFieldValue } from '@core/api-models/backlog-item/item/BacklogItemCustomFieldValue';
import { BacklogItemType } from '@core/api-models/common/backlog-item';
import { CustomFieldListGetResponse } from '@core/api-models/custom-field/list';
import { CustomFieldsService } from '@core/api-services/customfields.service';
import { orderBy } from 'lodash';
import { isEmpty } from 'lodash-es';
import { filter, map, take } from 'rxjs/operators';
import { CustomFieldsAddDialogComponent, ICustomFieldsAddDialogParams } from './add-dialog/custom-fields-add-dialog.component';

@Component({
	selector: 'backlog-item-custom-fields',
	templateUrl: './custom-fields.component.html',
	styleUrls: ['./custom-fields.component.scss'],
})
export class BacklogItemCustomFieldsComponent implements ControlValueAccessor {
	@Input()
	currentBacklogItemId: string | undefined | null;
	@Input()
	customFields: BacklogItemCustomFieldValue[] | undefined;
	@Input()
	set backlogItemType(val: keyof typeof BacklogItemType | undefined) {
		this._backlogItemType = val;
		if (!!this._backlogItemType)
			this.apiService
				.getCustomFieldList({ backlogItemType: this._backlogItemType })
				.pipe(
					map(i => i.entries.filter(e => !this.customFields || !this.customFields.find(c => c.customFieldId == e.id))),
					take(1)
				)
				.subscribe(f => (this._availableFields = !isEmpty(f) ? f : undefined));
	}
	private _backlogItemType: keyof typeof BacklogItemType | undefined;

	get customFieldsOrdered(): BacklogItemCustomFieldValue[] | undefined {
		return !!this.customFields ? orderBy(this.customFields, c => c.name) : undefined;
	}

	get value(): BacklogCustomFieldAction[] | undefined {
		return this._value;
	}
	set value(val: BacklogCustomFieldAction[] | undefined) {
		this._value = val;
		this._onChange(val);
	}
	private _value: BacklogCustomFieldAction[] | undefined;

	isDisabled = false;

	public get availableFields(): CustomFieldListGetResponse[] | undefined {
		return this._availableFields;
	}
	private _availableFields: CustomFieldListGetResponse[] | undefined;

	/* View -> model callback called when select has been touched */
	private _onTouched: () => void = () => {};
	/* View -> model callback called when value changes from the UI */
	private _onChange: (value?: BacklogCustomFieldAction[]) => void = () => {};

	constructor(
		/* Based on https://material.angular.io/guide/creating-a-custom-form-field-control#ngcontrol */
		@Optional() @Self() private ngControl: NgControl,
		private dialog: MatDialog,
		private apiService: CustomFieldsService
	) {
		if (this.ngControl != null) {
			// Setting the value accessor directly (instead of using the providers) to avoid running into a circular import
			this.ngControl.valueAccessor = this;
		}
	}

	openAddFieldDialog(): void {
		if (!this._availableFields) return;
		const params: ICustomFieldsAddDialogParams = {
			availableFields: this._availableFields,
		};
		this.dialog
			.open(CustomFieldsAddDialogComponent, { data: params, minWidth: '400px' })
			.afterClosed()
			.pipe(
				filter((l: BacklogCustomFieldAction) => !!l),
				take(1)
			)
			.subscribe(l => {
				const field = this._availableFields?.find(x => x.id == l.customFieldId);
				if (!field) return;

				if (!this.value?.length) this.value = [l as BacklogCustomFieldAction];
				else this.value.push(l as BacklogCustomFieldAction);

				const fieldVal: BacklogItemCustomFieldValue = {
					customFieldId: l.customFieldId,
					value: l.value,
					name: field.name,
					type: field.fieldType,
					isMandatory: field.isMandatory,
				};

				if (!this.customFields?.length) this.customFields = [fieldVal];
				else this.customFields.push(fieldVal);
			});
	}

	writeValue(value?: BacklogCustomFieldAction[]): void {
		this._value = value;
	}
	registerOnChange(fn: (value?: BacklogCustomFieldAction[]) => void): void {
		this._onChange = fn;
	}
	registerOnTouched(fn: () => {}): void {
		this._onTouched = fn;
	}
	setDisabledState(isDisabled: boolean): void {
		this.isDisabled = isDisabled;
	}
}
