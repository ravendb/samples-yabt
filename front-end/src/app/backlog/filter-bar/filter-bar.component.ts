import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { BacklogItemListGetRequest } from '@core/models/backlog-item/list/BacklogItemListGetRequest';
import { CurrentUserRelations } from '@core/models/backlog-item/list/CurrentUserRelations';
import { IKeyValuePair } from '@shared/filters';
import { map } from 'lodash-es';
import { merge } from 'rxjs';
import { delay, distinctUntilChanged, filter, take } from 'rxjs/operators';
import { BacklogFilterDialogComponent } from '../filter-dialog';
import { FilterBarComponentBase } from './filter-bar-base.component';

@Component({
	selector: 'filter-bar',
	styleUrls: ['./filter-bar.component.scss'],
	templateUrl: './filter-bar.component.html',
})
export class FilterBarComponent extends FilterBarComponentBase<BacklogItemListGetRequest> implements OnInit, OnDestroy {
	modes: IKeyValuePair[] = Object.keys(CurrentUserRelations).map(key => {
		return { key, value: CurrentUserRelations[key as keyof typeof CurrentUserRelations] };
	});

	constructor(private dialog: MatDialog, fb: FormBuilder) {
		super(fb);
	}

	ngOnInit(): void {
		super.ngOnInit();

		const triggers = map(this.formGroup.controls, (c: AbstractControl) => c.valueChanges.pipe(distinctUntilChanged()));

		this.subscription.add(
			merge(...triggers)
				.pipe(delay(0))
				.subscribe(() => this.applyFilter())
		);
	}

	ngOnDestroy(): void {
		this.subscription.unsubscribe();
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
