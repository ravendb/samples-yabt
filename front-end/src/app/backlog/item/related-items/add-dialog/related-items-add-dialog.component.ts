import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { BacklogRelationshipAction } from '@core/api-models/backlog-item/item/BacklogRelationshipAction';
import { BacklogRelationshipActionType } from '@core/api-models/backlog-item/item/BacklogRelationshipActionType';
import { BacklogRelationshipType } from '@core/api-models/common/backlog-item';
import { BacklogItemsService } from '@core/api-services/backlogItems.service';
import { IKeyValuePair } from '@shared/filters';
import { CustomValidators } from '@utils/custom-validators';
import { map } from 'rxjs/operators';

@Component({
	templateUrl: './related-items-add-dialog.component.html',
	styleUrls: ['./related-items-add-dialog.component.scss'],
})
export class RelatedItemsAddDialogComponent implements OnInit {
	readonly linkTypes: IKeyValuePair[] = Object.keys(BacklogRelationshipType).map(key => {
		return { key, value: BacklogRelationshipType[key as keyof typeof BacklogRelationshipType] };
	});

	form!: FormGroupTyped<BacklogRelationshipAction>;

	backlogItems = this.backlogService.getBacklogItemList().pipe(map(list => list.entries.map(t => t.title)));

	constructor(
		private fb: FormBuilder,
		private backlogService: BacklogItemsService,
		private dialogRef: MatDialogRef<RelatedItemsAddDialogComponent>
	) {}

	save(): void {
		this.dialogRef.close();
	}
	close(): void {
		this.dialogRef.close();
	}

	backlogItemSelected(): void {}

	ngOnInit(): void {
		const add: keyof typeof BacklogRelationshipActionType = 'add';
		this.form = this.fb.group({
			backlogItemId: [null, [CustomValidators.required()]],
			relationType: [null, [CustomValidators.required()]],
			actionType: [add, [CustomValidators.required()]],
		}) as FormGroupTyped<BacklogRelationshipAction>;
	}
}
