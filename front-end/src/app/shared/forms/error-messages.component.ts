import { Component, Input, ViewChild } from '@angular/core';
import { AbstractControl, ControlContainer, ControlValueAccessor, FormControlDirective, NG_VALUE_ACCESSOR } from '@angular/forms';
import { get, has } from 'lodash-es';

@Component({
	selector: 'error-messages',
	template: `<span *ngFor="let item of filteredErrors"> {{ item }}<br /></span>`,
	providers: [
		{
			provide: NG_VALUE_ACCESSOR,
			useExisting: ErrorMessagesComponent,
			multi: true,
		},
	],
})
export class ErrorMessagesComponent implements ControlValueAccessor {
	@ViewChild(FormControlDirective, { static: true })
	formControlDirective!: FormControlDirective;
	@Input()
	formControl: FormControl | undefined;
	@Input()
	formControlName: string | undefined;

	// Substitute messages for these error types
	private replacements: { [id: string]: string } = {
		matDatepickerMax: 'Date must be before {max}',
		matDatepickerMin: 'Date must be after {min}',
		matDatepickerParse: 'Invalid date',
		min: 'Cannot be less than {min}',
		required: 'This field is required',
	};

	get filteredErrors(): string[] {
		const ctrl = this.getControl();
		if (!ctrl || !ctrl.errors) return [];

		return Object.keys(ctrl.errors || {}).map((key: string) => this.processedMessage(key, ctrl.errors![key]));
	}

	constructor(private controlContainer: ControlContainer) {}

	private processedMessage(key: string, originalValue: string): string {
		return !has(this.replacements, key)
			? originalValue
			: this.replacements[key].replace(/{([^}]*)}/, (str: string, match: string) => {
					let value = get(originalValue, match, '');
					return value;
			  });
	}

	/* 
		Resolve FormControl instance no matter `formControl` or `formControlName` is given. 
		If formControlName is given, then this.controlContainer.control is the parent FormGroup (or FormArray) instance. 
	*/
	private getControl(): AbstractControl | undefined {
		return this.formControl || (!!this.formControlName && this.controlContainer.control?.get(this.formControlName)) || undefined;
	}

	/* As we don't manipulate with control, provide a stub on 'ControlValueAccessor' implementation */
	registerOnChange(_: (value?: string) => void): void {}
	registerOnTouched(_: () => {}): void {}
	writeValue(_: string): void {}
}
