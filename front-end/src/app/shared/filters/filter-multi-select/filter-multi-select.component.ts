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
		this._subscriptions.add(this.list.selectionChange.subscribe(() => this.selectionChanged()));
	}

	clear(): void {
		this.writeValue();
		this._onChange();
	}

	writeValue(value?: string[]): void {
		super.writeValue(value);
		this.initiateList();
	}

	protected updateLabel(keys?: string[]): void {
		if (!keys || isEmpty(keys) || isEmpty(this.options)) this.buttonText = this.label;
		else {
			if (!isArray(keys)) keys = [keys];
			const values = this.options.filter(o => keys!.includes(o.key)).map(o => o.value);

			let newButtonText = values.find(Boolean) || '';
			if (values.length > 1) newButtonText += ' +1';
			this.buttonText = newButtonText;

			this.rectangleText = values.join(', ');
		}
	}

	protected initiateList(): void {
		if (!isEmpty(this.options) && !isEmpty(this.list?.options)) {
			const selectedKeys = this.getSanitisedValues();
			this.list.options.forEach(option => option._setSelected(selectedKeys.findIndex(key => key === option.value) > -1));
		}
	}

	private selectionChanged(): void {
		const keys: string[] = compact(
			this.list.selectedOptions.selected.map(option => this.options.find(item => option.value === item.key)?.key)
		);
		this.writeValue(keys);
		this._onChange(keys);
	}

	private getSanitisedValues(): string[] {
		if (!this.value) return [];
		const selectedKeys = isArray(this.value) ? this.value : isString(this.value) ? [this.value as string] : [];
		return selectedKeys;
	}
}
