import { Component, Input, OnInit, Optional, Self } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { BacklogCustomFieldAction } from '@core/api-models/backlog-item/item/BacklogCustomFieldAction';
import { BacklogItemCustomFieldValue } from '@core/api-models/backlog-item/item/BacklogItemCustomFieldValue';
import { CustomFieldsService } from '@core/api-services/customfields.service';
import { orderBy } from 'lodash';

@Component({
	selector: 'backlog-item-custom-fields',
	templateUrl: './custom-fields.component.html',
	styleUrls: ['./custom-fields.component.scss'],
})
export class BacklogItemCustomFieldsComponent implements ControlValueAccessor, OnInit {
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
		/*		this.dialog
			.open(RelatedItemsAddDialogComponent, { data: this.currentBacklogItemId, minWidth: '400px' })
			.afterClosed()
			.pipe(
				filter((l: BacklogRelationshipActionEx) => !!l),
				take(1)
			)
			.subscribe(l => {
				if (!this.value?.length) this.value = [l];
				else this.value.push(l);

				if (!this._initialRelatedItems?.length) this._initialRelatedItems = [this.getBacklogRelatedItem(l)];
				else this._initialRelatedItems.push(this.getBacklogRelatedItem(l));
				this.initialRelatedItems = this._initialRelatedItems;
			});
		*/
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
