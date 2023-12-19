import {AbstractControl, UntypedFormArray, UntypedFormBuilder, UntypedFormControl, UntypedFormGroup} from '@angular/forms';
import {FormGroupTyped} from "../../typings";

/**
 * Deep clones the given AbstractControl, preserving values, validators, async validators, and disabled status.
 * @param control AbstractControl
 * @returns AbstractControl
 *
 * Taken from https://stackoverflow.com/a/49743369/968003
 */
export function cloneAbstractControl<T extends AbstractControl>(control: T): T {
	let newControl: T;

	if (control instanceof UntypedFormGroup) {
		const formGroup = new UntypedFormGroup({}, control.validator, control.asyncValidator);
		const controls = control.controls;

		Object.keys(controls).forEach(key => {
			formGroup.addControl(key, cloneAbstractControl(controls[key]));
		});

		newControl = formGroup as any;
	} else if (control instanceof UntypedFormArray) {
		const formArray = new UntypedFormArray([], control.validator, control.asyncValidator);

		control.controls.forEach(formControl => formArray.push(cloneAbstractControl(formControl)));

		newControl = formArray as any;
	} else if (control instanceof UntypedFormControl) {
		newControl = new UntypedFormControl(control.value, control.validator, control.asyncValidator) as any;
	} else {
		throw new Error('Error: unexpected control value');
	}

	if (control.disabled) newControl.disable({ emitEvent: false });

	return newControl;
}

/*
 * Create a strongly typed FromGroup from an object
 */
export function generateFormGroupFromObject<T>(fb: UntypedFormBuilder, obj: Partial<T>): FormGroupTyped<T> {
	return fb.group(
		Object.keys(obj).reduce((prev, curr) => Object.assign(prev, { [curr]: obj[curr as keyof T] instanceof Array ? [] : null }), {})
	) as FormGroupTyped<T>;
}
