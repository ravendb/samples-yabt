import { Component, Input, Optional, Self } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { BacklogRelationshipAction } from '@core/api-models/backlog-item/item/BacklogRelationshipAction';
import { BacklogItemRelatedItem, BacklogRelationshipType } from '@core/api-models/common/backlog-item';
import { BacklogItemReference } from '@core/api-models/common/references';
import { filter, take } from 'rxjs/operators';
import { RelatedItemsAddDialogComponent } from './add-dialog';
import { BacklogRelationshipActionEx } from './BacklogRelationshipActionEx';
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
		this._initialRelatedItems = val;
	}
	private _initialRelatedItems: BacklogItemRelatedItem[] | undefined;

	@Input()
	currentBacklogItemId: string | undefined | null;

	get value(): BacklogRelationshipAction[] | undefined {
		return this._value;
	}
	set value(val: BacklogRelationshipAction[] | undefined) {
		this._value = val;
		this._onChange(val);
	}
	private _value: BacklogRelationshipAction[] | undefined;

	isDisabled = false;

	public get relationshipType(): typeof BacklogRelationshipType {
		return BacklogRelationshipType;
	}

	/* View -> model callback called when select has been touched */
	private _onTouched: () => void = () => {};
	/* View -> model callback called when value changes from the UI */
	private _onChange: (value?: BacklogRelationshipAction[]) => void = () => {};

	groupedItems: Record<keyof typeof BacklogRelationshipType, BacklogItemReference[]> | undefined;

	get types(): Array<keyof typeof BacklogRelationshipType> {
		return !!this.groupedItems ? (Object.keys(this.groupedItems) as Array<keyof typeof BacklogRelationshipType>) : [];
	}

	constructor(
		/* Based on https://material.angular.io/guide/creating-a-custom-form-field-control#ngcontrol */
		@Optional() @Self() private ngControl: NgControl,
		private dialog: MatDialog
	) {
		if (this.ngControl != null) {
			// Setting the value accessor directly (instead of using the providers) to avoid running into a circular import
			this.ngControl.valueAccessor = this;
		}
	}

	openAddLinkDialog(): void {
		this.dialog
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
	}

	writeValue(value?: BacklogRelationshipAction[]): void {
		this._value = value;
	}
	registerOnChange(fn: (value?: BacklogRelationshipAction[]) => void): void {
		this._onChange = fn;
	}
	registerOnTouched(fn: () => {}): void {
		this._onTouched = fn;
	}
	setDisabledState(isDisabled: boolean): void {
		this.isDisabled = isDisabled;
	}

	remove(backlogItemId: string, relationType: keyof typeof BacklogRelationshipType): void {
		const action: BacklogRelationshipAction = { actionType: 'remove', backlogItemId, relationType };
		if (!this.value?.length) this.value = [action];
		else this.value.push(action);

		if (!this._initialRelatedItems?.length) return;
		this.initialRelatedItems = this._initialRelatedItems.filter(
			x => x.relatedTo?.id != action.backlogItemId || x.linkType != relationType
		);
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

	private getBacklogRelatedItem(b: BacklogRelationshipActionEx): BacklogItemRelatedItem {
		return {
			linkType: b.relationType,
			relatedTo: { id: b.backlogItemId, name: b.backlogItemTitle, type: b.backlogItemType } as BacklogItemReference,
		} as BacklogItemRelatedItem;
	}
}
