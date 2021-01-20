import { Component } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { BacklogItemListGetRequest } from '@core/api-models/backlog-item/list/BacklogItemListGetRequest';
import { CurrentUserRelations } from '@core/api-models/backlog-item/list/CurrentUserRelations';
import { IKeyValuePair } from '@shared/filters';
import { FilterBarComponentBase } from '@shared/filters/filter-bar-base.component';
import { filter, take } from 'rxjs/operators';
import { BacklogFilterDialogComponent } from '../filter-dialog';

@Component({
	selector: 'filter-bar',
	styleUrls: ['./filter-bar.component.scss'],
	templateUrl: './filter-bar.component.html',
})
export class FilterBarComponent extends FilterBarComponentBase<BacklogItemListGetRequest> {
	modes: IKeyValuePair[] = Object.keys(CurrentUserRelations).map(key => {
		return { key, value: CurrentUserRelations[key as keyof typeof CurrentUserRelations] };
	});

	constructor(private dialog: MatDialog, fb: FormBuilder) {
		super(fb);
	}

	openFilterDialog(): void {
		this.dialog
			.open(BacklogFilterDialogComponent, { data: this.formGroup.value, minWidth: '350px' })
			.afterClosed()
			.pipe(
				take(1),
				filter(d => !!d)
			)
			.subscribe(result => {
				this.filterChange.emit(result);
			});
	}
}
