import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { isNil, uniq } from 'lodash-es';

// Extension of https://github.com/angular/angular/blob/master/packages/forms/src/validators.ts
// tslint:disable-next-line:max-line-length
const emailRegex = /^(?=.{1,254}$)(?=.{1,64}@)[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;

const urlRegex = /^(https?:\/\/)?([a-z0-9]+\.)+[a-z0-9]+[a-z0-9\-\/]*$/i;

export class CustomValidators {
	static required(fieldName?: string): ValidatorFn {
		return (control: AbstractControl): ValidationErrors | null => {
			return this.isEmptyInputValue(control.value) ? { customRequired: `${fieldName || 'This field'} must be entered` } : null;
		};
	}

	static requiredWhen(condition: () => boolean, fieldName?: string): ValidatorFn {
		return (control: AbstractControl): ValidationErrors | null => {
			return condition() ? this.required(fieldName)(control) : null;
		};
	}

	static email(): ValidatorFn {
		return (control: AbstractControl): ValidationErrors | null => {
			return emailRegex.test(control.value) ? null : { customEmail: 'Invalid email address' };
		};
	}

	static positiveNumber(): ValidatorFn {
		return (control: AbstractControl): ValidationErrors | null => {
			if (isNil(control.value)) return null;
			return control.value > 0 ? null : { customEmail: 'Enter a positive value' };
		};
	}

	static lessThanControlValue(ctrlName: string): ValidatorFn {
		return (control: AbstractControl): ValidationErrors | null => {
			if (!control || !control.parent) return null;
			const dependentCtrl = control.parent.get(ctrlName);

			return !dependentCtrl ||
				this.isEmptyInputValue(dependentCtrl.value) ||
				this.isEmptyInputValue(control.value) ||
				control.value <= dependentCtrl.value
				? null
				: { customMax: `Cannot be greater than ${dependentCtrl.value}` };
		};
	}

	static optionalEmail(): ValidatorFn {
		return (control: AbstractControl): ValidationErrors | null => {
			return !this.isEmptyInputValue(control.value) ? this.email()(control) : null;
		};
	}

	static url(): ValidatorFn {
		return (control: AbstractControl): ValidationErrors | null => {
			return urlRegex.test(control.value) ? null : { customUrl: 'Invalid URL' };
		};
	}

	static optionalUrl(): ValidatorFn {
		return (control: AbstractControl): ValidationErrors | null => {
			return !this.isEmptyInputValue(control.value) ? this.url()(control) : null;
		};
	}

	static pattern(regex: RegExp): ValidatorFn {
		return (control: AbstractControl): ValidationErrors | null => {
			return this.isEmptyInputValue(control.value) || regex.test(control.value) ? null : { customPattern: 'Invalid format' };
		};
	}

	static uniqueArrayField(fieldName: string, errorMessage: string = ''): ValidatorFn {
		return (control: AbstractControl): ValidationErrors | null => {
			const uniqueValues = uniq(control.value.map((item: any) => item[fieldName]));
			if (uniqueValues.length !== control.value.length) {
				return { uniqueArrayField: errorMessage ? errorMessage : 'Values must be unique' };
			} else {
				return null;
			}
		};
	}

	static nonEmptyArray(): ValidatorFn | null {
		return (control: AbstractControl): ValidationErrors | null => {
			if (this.isEmptyInputValue(control.value)) {
				return { nonEmpty: 'At least one value must be entered' };
			} else {
				return null;
			}
		};
	}

	private static isEmptyInputValue(value: string | []): boolean {
		return isNil(value) || value.length === 0;
	}
}
