import { COMMA, ENTER, SPACE } from '@angular/cdk/keycodes';
import { Component, ElementRef, ViewChild } from '@angular/core';
import { ControlContainer, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatAutocomplete, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatChipInputEvent } from '@angular/material/chips';
import { BacklogItemsService } from '@core/api-services/backlogItems.service';
import { CustomFormControlBase } from '@shared/forms/custom-form-control-base';
import { map } from 'rxjs/operators';

@Component({
	selector: 'backlog-item-tags-selector',
	templateUrl: './tags.component.html',
	styleUrls: ['./tags.component.scss'],
	providers: [
		{
			provide: NG_VALUE_ACCESSOR,
			useExisting: TagsComponent,
			multi: true,
		},
	],
})
export class TagsComponent extends CustomFormControlBase<string[]> {
	@ViewChild('tagsInput')
	tagsInput: ElementRef<HTMLInputElement> = null!;
	@ViewChild('auto')
	matAutocomplete: MatAutocomplete = null!;

	readonly separatorKeysCodes: number[] = [ENTER, COMMA, SPACE];

	serverTags = this.backlogService.getBacklogItemTagList().pipe(map(tags => tags.map(t => t.name)));

	constructor(controlContainer: ControlContainer, private backlogService: BacklogItemsService) {
		super(controlContainer);
	}

	add(event: MatChipInputEvent): void {
		const input = event.input;
		const value = event.value;

		// Add a new tag
		if ((value || '').trim()) {
			this.mainControl.setValue([...this.mainControl.value, value.trim()]);
			this.mainControl.updateValueAndValidity();
		}

		// Reset the input value
		if (!!input) {
			input.value = '';
		}
	}

	remove(tag: string): void {
		const index = this.mainControl.value.indexOf(tag);

		if (index >= 0) {
			this.mainControl.value.splice(index, 1);
			this.mainControl.updateValueAndValidity();
		}
	}

	selected(event: MatAutocompleteSelectedEvent): void {
		this.add({ input: this.tagsInput.nativeElement, value: event.option.viewValue });
	}
}
