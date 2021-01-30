import { Component, Input } from '@angular/core';
import { AbstractControl } from '@angular/forms';
import { get, has } from 'lodash-es';

@Component({
	selector: 'error-messages',
	template: `<span *ngFor="let item of filteredErrors"> {{ item }}<br /></span>`,
})
export class ErrorMessagesComponent {
	@Input()
	for!: AbstractControl;

	// Substitute messages for these error types
	private replacements: { [id: string]: string } = {
		matDatepickerMax: 'Date must be before {max}',
		matDatepickerMin: 'Date must be after {min}',
		matDatepickerParse: 'Invalid date',
		min: 'Cannot be less than {min}',
		required: 'This field is required',
	};

	get filteredErrors(): string[] {
		if (!this.for || !this.for.errors) {
			return [];
		}

		return Object.keys(this.for.errors || {}).map((key: string) => this.processedMessage(key, this.for.errors![key]));
	}

	private processedMessage(key: string, originalValue: string): string {
		return !has(this.replacements, key)
			? originalValue
			: this.replacements[key].replace(/{([^}]*)}/, (str: string, match: string) => {
					let value = get(originalValue, match, '');
					return value;
			  });
	}
}
