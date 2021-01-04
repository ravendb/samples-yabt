import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { IKeyValuePair } from '../ikey-value-pair';

@Component({
	selector: 'filter-single-select',
	styleUrls: ['./filter-single-select.component.scss'],
	templateUrl: './filter-single-select.component.html',
})
export class FilterSingleSelectComponent implements OnInit, OnDestroy {
	@Input()
	get label(): string {
		return this._label;
	}
	set label(value: string) {
		this._label = value;
		this.updateLabel(this.control?.value);
	}
	@Input()
	buttonAltText: string = '';
	@Input()
	options: IKeyValuePair[] = [];
	@Input()
	firstUndefinedOption: string = '';
	@Input()
	control!: AbstractControl;

	buttonText: string = this.label;

	private _label: string = 'Unspecified';
	private _subscriptions: Subscription = new Subscription();

	setType(value: string | null): void {
		const el = !!value ? this.options.find(v => v.key == value) : undefined;
		this.buttonText = el?.value || this.label;
		this.control.setValue(el?.key || undefined);
	}

	ngOnInit() {
		this._subscriptions.add(this.control.valueChanges.subscribe((key: string) => this.updateLabel(key)));
	}

	ngOnDestroy() {
		this._subscriptions.unsubscribe();
	}

	private updateLabel(key: string): void {
		const el = !!key ? this.options.find(v => v.key == key) : undefined;
		this.buttonText = el?.value || this.label;
	}
}
