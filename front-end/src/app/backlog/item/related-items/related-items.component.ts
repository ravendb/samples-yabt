import { Component, Input, Optional, Self } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';
import { BacklogItemRelatedItem, BacklogRelationshipType } from '@core/api-models/common/backlog-item';
import { BacklogItemReference } from '@core/api-models/common/references';

@Component({
	selector: 'backlog-item-related-items',
	templateUrl: './related-items.component.html',
	styleUrls: ['./related-items.component.scss'],
})
export class BacklogItemRelatedItemsComponent implements ControlValueAccessor {
	@Input()
	set initialRelatedItems(val: BacklogItemRelatedItem[] | undefined) {
		this.groupedItems = this.getGroupedItems(
			val,
			i => i.linkType,
			i => i.relatedTo!
		);
	}

	get value(): BacklogItemRelatedItem[] | undefined {
		return this._value;
	}
	set value(val: BacklogItemRelatedItem[] | undefined) {
		this._value = val;
		this._onChange(val);
	}
	private _value: BacklogItemRelatedItem[] | undefined;

	isDisabled = false;

	public get relationshipType(): typeof BacklogRelationshipType {
		return BacklogRelationshipType;
	}

	/* View -> model callback called when select has been touched */
	private _onTouched: () => void = () => {};
	/* View -> model callback called when value changes from the UI */
	private _onChange: (value?: BacklogItemRelatedItem[]) => void = () => {};

	groupedItems: Record<keyof typeof BacklogRelationshipType, BacklogItemReference[]> | undefined;

	get types(): Array<keyof typeof BacklogRelationshipType> {
		return !!this.groupedItems ? (Object.keys(this.groupedItems) as Array<keyof typeof BacklogRelationshipType>) : [];
	}

	constructor(
		/* Based on https://material.angular.io/guide/creating-a-custom-form-field-control#ngcontrol */
		@Optional() @Self() private ngControl: NgControl
	) {
		if (this.ngControl != null) {
			// Setting the value accessor directly (instead of using the providers) to avoid running into a circular import
			this.ngControl.valueAccessor = this;
		}
	}

	writeValue(value?: BacklogItemRelatedItem[]): void {
		this._value = value;
	}
	registerOnChange(fn: (value?: BacklogItemRelatedItem[]) => void): void {
		this._onChange = fn;
	}
	registerOnTouched(fn: () => {}): void {
		this._onTouched = fn;
	}
	setDisabledState(isDisabled: boolean): void {
		this.isDisabled = isDisabled;
	}

	private getGroupedItems(
		list: BacklogItemRelatedItem[] | undefined,
		getKey: (item: BacklogItemRelatedItem) => keyof typeof BacklogRelationshipType,
		getResult: (item: BacklogItemRelatedItem) => BacklogItemReference
	): Record<keyof typeof BacklogRelationshipType, BacklogItemReference[]> {
		if (!list) return {} as Record<keyof typeof BacklogRelationshipType, BacklogItemReference[]>;

		return list.reduce((previous, currentItem) => {
			const group = getKey(currentItem);
			if (!previous[group]) previous[group] = [];
			previous[group].push(getResult(currentItem));
			return previous;
		}, {} as Record<keyof typeof BacklogRelationshipType, BacklogItemReference[]>);
	}
}
