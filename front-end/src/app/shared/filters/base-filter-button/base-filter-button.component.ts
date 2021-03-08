import { Directive, Input, Optional, Self } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';
import { IKeyValuePair } from '../ikey-value-pair';

@Directive()
export abstract class BaseFilterButtonComponent<TKey> implements ControlValueAccessor {
	@Input()
	get label(): string {
		return this._label;
	}
	set label(value: string) {
		this._label = value;
		this.updateLabel(this._value);
	}
	@Input()
	buttonAltText: string = '';
	@Input()
	options: IKeyValuePair[] = [];
	@Input()
	stretchedAndStroked: boolean = false;

	buttonText: string = this.label;
	isDisabled = false;

	get value(): TKey | undefined {
		return this._value;
	}
	set value(val: TKey | undefined) {
		this._value = val;
		this._onChange(val);
		this.updateLabel(val);
	}
	private _value: TKey | undefined;

	protected _label: string = 'Unspecified';

	/* View -> model callback called when select has been touched */
	protected _onTouched: () => void = () => {};
	/* View -> model callback called when value changes from the UI */
	protected _onChange: (value?: TKey) => void = () => {};

	constructor(
		/* Based on https://material.angular.io/guide/creating-a-custom-form-field-control#ngcontrol */
		@Optional() @Self() private ngControl: NgControl
	) {
		if (this.ngControl != null) {
			// Setting the value accessor directly (instead of using the providers) to avoid running into a circular import
			this.ngControl.valueAccessor = this;
		}
	}

	writeValue(value?: TKey): void {
		this._value = value;
		this.updateLabel(value);
	}
	registerOnChange(fn: (value?: TKey) => void): void {
		this._onChange = fn;
	}
	registerOnTouched(fn: () => {}): void {
		this._onTouched = fn;
	}
	setDisabledState(isDisabled: boolean): void {
		this.isDisabled = isDisabled;
	}

	protected abstract updateLabel(keys?: TKey): void;
}
