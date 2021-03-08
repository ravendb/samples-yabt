import { Directive, Input, ViewChild } from '@angular/core';
import { ControlContainer, ControlValueAccessor, FormControl, FormControlDirective } from '@angular/forms';

@Directive()
export class CustomFormControlBase<T> implements ControlValueAccessor {
	@ViewChild(FormControlDirective, { static: true })
	formControlDirective!: FormControlDirective;
	@Input()
	formControl: FormControl | undefined;
	@Input()
	formControlName: string | undefined;

	/* 
		Resolve FormControl instance no matter `formControl` or `formControlName` is given. 
		If formControlName is given, then this.controlContainer.control is the parent FormGroup (or FormArray) instance. 
	*/
	get control(): AbstractControl | undefined {
		return this.formControl || (!!this.formControlName && this.controlContainer.control?.get(this.formControlName)) || undefined;
	}

	readonly mainControl: FormControlTyped<T> = new FormControl();

	/* View -> model callback called when select has been touched */
	public onTouched: () => void = () => {};
	/* View -> model callback called when value changes from the UI */
	protected _onChange: (value?: T) => void = () => {};

	constructor(private controlContainer: ControlContainer) {}

	registerOnChange(fn: (value?: T) => void): void {
		this._onChange = fn;
	}
	registerOnTouched(fn: () => {}): void {
		this.onTouched = fn;
	}
	writeValue(val: T): void {
		this.mainControl.setValue(val, { emitEvent: false });
	}
	setDisabledState(isDisabled: boolean): void {
		if (isDisabled) this.mainControl.disable();
		else this.mainControl.enable();
	}
}
