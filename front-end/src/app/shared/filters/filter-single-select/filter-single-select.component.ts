import { Component, ContentChild, Input, OnDestroy, OnInit, TemplateRef } from '@angular/core';
import { BaseFilterButtonComponent } from '../base-filter-button';
import { IKeyValuePair } from '../ikey-value-pair';

@Component({
	selector: 'filter-single-select',
	styleUrls: ['./filter-single-select.component.scss'],
	templateUrl: './filter-single-select.component.html',
})
export class FilterSingleSelectComponent extends BaseFilterButtonComponent<string, IKeyValuePair> implements OnInit, OnDestroy {
	@ContentChild(TemplateRef)
	templateRef: TemplateRef<any> | undefined;

	@Input()
	firstUndefinedOption: string = '';

	setType(value: string | null): void {
		const el = !!value ? this.options.find(v => v.key == value) : undefined;
		this.buttonText = el?.value || this.label;
		this.control.setValue(el?.key || undefined);
	}

	public isSelected(key: string): boolean {
		return this.control.value == key;
	}

	protected updateLabel(key: string): void {
		const el = !!key ? this.options.find(v => v.key == key) : undefined;
		this.buttonText = el?.value || this.label;
	}
}
