import { Component, Input, OnInit, Optional, Self } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { BacklogCustomFieldAction } from '@core/api-models/backlog-item/item/BacklogCustomFieldAction';
import { BacklogItemCustomFieldValue } from '@core/api-models/backlog-item/item/BacklogItemCustomFieldValue';
import { BacklogItemType } from '@core/api-models/common/backlog-item';
import { CustomFieldsService } from '@core/api-services/customfields.service';
import { orderBy } from 'lodash';
import { filter, take } from 'rxjs/operators';
import { CustomFieldsAddDialogComponent, ICustomFieldsAddDialogParams } from './add-dialog/custom-fields-add-dialog.component';

@Component({
	selector: 'backlog-item-custom-fields',
	templateUrl: './custom-fields.component.html',
	styleUrls: ['./custom-fields.component.scss'],
})
export class BacklogItemCustomFieldsComponent implements ControlValueAccessor, OnInit {
	@Input()
	currentBacklogItemId: string | undefined | null;
	@Input()
	backlogItemType: keyof typeof BacklogItemType | undefined | null;
	@Input()
	customFields: BacklogItemCustomFieldValue[] | undefined;

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
	ngOnInit(): void {}

	openAddFieldDialog(): void {
		if (!this.currentBacklogItemId || !this.backlogItemType) return;
		const params: ICustomFieldsAddDialogParams = {
			backlogItemId: this.currentBacklogItemId,
			currentFieldIds: this.customFields?.map(cf => cf.customFieldId),
			backlogItemType: this.backlogItemType,
		};
		this.dialog
			.open(CustomFieldsAddDialogComponent, { data: params, minWidth: '400px' })
			.afterClosed()
			.pipe(
				filter((l: BacklogCustomFieldAction | BacklogItemCustomFieldValue) => !!l),
				take(1)
			)
			.subscribe(l => {
				if (!this.value?.length) this.value = [l as BacklogCustomFieldAction];
				else this.value.push(l as BacklogCustomFieldAction);

				if (!this.customFields?.length) this.customFields = [l as BacklogItemCustomFieldValue];
				else this.customFields.push(l as BacklogItemCustomFieldValue);
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
