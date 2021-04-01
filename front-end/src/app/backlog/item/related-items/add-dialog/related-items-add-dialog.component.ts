import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { BacklogItemListGetResponse } from '@core/api-models/backlog-item/list';
import { BacklogRelationshipType } from '@core/api-models/common/backlog-item';
import { ListActionType } from '@core/api-models/common/ListActionType';
import { BacklogItemsService } from '@core/api-services/backlogItems.service';
import { IKeyValuePair } from '@shared/filters';
import { CustomValidators } from '@utils/custom-validators';
import { debounceTime, distinctUntilChanged, map, switchMap } from 'rxjs/operators';
import { BacklogRelationshipActionEx } from '../BacklogRelationshipActionEx';

@Component({
	templateUrl: './related-items-add-dialog.component.html',
	styleUrls: ['./related-items-add-dialog.component.scss'],
})
export class RelatedItemsAddDialogComponent implements OnInit {
	readonly linkTypes: IKeyValuePair[] = Object.keys(BacklogRelationshipType).map(key => {
		return { key, value: BacklogRelationshipType[key as keyof typeof BacklogRelationshipType] };
	});

	form!: FormGroupTyped<BacklogRelationshipActionEx>;

	backlogItems: BacklogItemListGetResponse[] | undefined;

	constructor(
		@Inject(MAT_DIALOG_DATA) public currentBacklogItemId: string | undefined,
		private fb: FormBuilder,
		private backlogService: BacklogItemsService,
		private dialogRef: MatDialogRef<RelatedItemsAddDialogComponent>
	) {}

	ngOnInit(): void {
		const add: keyof typeof ListActionType = 'add';
		this.form = this.fb.group({
			backlogItemId: [null, [CustomValidators.required()]],
			backlogItemTitle: [null, [CustomValidators.required()]],
			backlogItemType: [null, [CustomValidators.required()]],
			relationType: [null, [CustomValidators.required()]],
			actionType: [add, [CustomValidators.required()]],
		}) as FormGroupTyped<BacklogRelationshipActionEx>;

		this.form.controls.backlogItemTitle.valueChanges
			.pipe(
				distinctUntilChanged(),
				debounceTime(300),
				switchMap(v => this.backlogService.getBacklogItemList({ search: v, pageSize: 20 }, false)),
				map(list => list.entries.filter(e => !this.currentBacklogItemId || e.id != this.currentBacklogItemId))
			)
			.subscribe(items => (this.backlogItems = items));
		this.form.controls.backlogItemTitle.setValue('');
	}

	addLink(): void {
		this.dialogRef.close(this.form.value);
	}
	close(): void {
		this.dialogRef.close();
	}

	backlogItemSelected(event: MatAutocompleteSelectedEvent): void {
		this.form.controls.backlogItemId.setValue(event.option.value);
		const item = this.backlogItems?.find(i => i.id == event.option.value);
		if (!!item) {
			this.form.controls.backlogItemTitle.setValue(item.title);
			this.form.controls.backlogItemType.setValue(item.type);
		} else {
			this.form.controls.backlogItemTitle.reset();
			this.form.controls.backlogItemType.reset();
		}
	}
}
