import { Component, ContentChild, Input, OnDestroy, OnInit, TemplateRef } from '@angular/core';
import { BaseSearchableFilterButtonComponent } from '../base-filter-button';

@Component({
	selector: 'filter-single-select',
	styleUrls: ['./filter-single-select.component.scss'],
	templateUrl: './filter-single-select.component.html',
})
export class FilterSingleSelectComponent extends BaseSearchableFilterButtonComponent<string> implements OnInit, OnDestroy {
	@ContentChild(TemplateRef)
	templateRef: TemplateRef<any> | undefined;

	@Input()
	firstUndefinedOption: string = '';
	@Input()
	narrowUnderscored: boolean = false;
	@Input()
	hideClearButton: boolean = false;

	setValue(value?: string): void {
		const el = !!value ? this.options.find(v => v.key == value) : undefined;
		this.writeValue(el?.key);
		this._onChange(el?.key);
	}

	clear(): void {
		this.writeValue();
		this._onChange();
	}

	protected initiateList(): void {}

	protected updateLabel(key: string): void {
		const el = !!key ? this.options.find(v => v.key == key) : undefined;
		this.buttonText = el?.value?.concat(!el?.key ? ' (deleted)' : '') || this.label;
	}
}
