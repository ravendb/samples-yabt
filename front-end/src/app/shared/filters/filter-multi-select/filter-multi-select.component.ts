import { AfterViewInit, Component, ContentChild, OnDestroy, TemplateRef, ViewChild } from '@angular/core';
import { MatSelectionList } from '@angular/material/list';
import { compact, isArray, isEmpty, isString } from 'lodash-es';
import { BaseSearchableFilterButtonComponent } from '../base-filter-button';

@Component({
	selector: 'filter-multi-select',
	styleUrls: ['./filter-multi-select.component.scss'],
	templateUrl: './filter-multi-select.component.html',
})
export class FilterMultiSelectComponent extends BaseSearchableFilterButtonComponent<string[]> implements AfterViewInit, OnDestroy {
	@ViewChild(MatSelectionList)
	list!: MatSelectionList;
	@ContentChild(TemplateRef)
	templateRef: TemplateRef<any> | undefined;

	rectangleText: string = '';

	ngAfterViewInit(): void {
		this._subscriptions.add(this.list.selectionChange.subscribe(() => this.applyFilter()));
	}

	applyFilter(): void {
		const keys: string[] = compact(
			this.list.selectedOptions.selected.map(option => this.options.find(item => option.value === item.key)?.key)
		);

		this.control.setValue(keys || undefined);
	}

	clear(): void {
		this.list.deselectAll();
		this.applyFilter();
	}

	protected updateLabel(keys: string[]): void {
		if (isEmpty(keys)) this.buttonText = this.label;
		else {
			if (!isArray(keys)) keys = [keys];
			const values = this.options.filter(o => keys.includes(o.key)).map(o => o.value);

			let newButtonText = values.find(Boolean) || '';
			if (values.length > 1) newButtonText += ' +1';
			this.buttonText = newButtonText;

			this.rectangleText = values.join(', ');
		}
		if (!isEmpty(this.options) && !isEmpty(this.list.options)) {
			const selectedKeys = this.getSetValues();
			this.list.options.forEach(option => option._setSelected(selectedKeys.findIndex(key => key === option.value) > -1));
		}
	}

	private getSetValues(): string[] {
		const selectedKeys = isArray(this.control.value)
			? this.control.value
			: isString(this.control.value)
			? [this.control.value as string]
			: [];
		return selectedKeys;
	}
}
