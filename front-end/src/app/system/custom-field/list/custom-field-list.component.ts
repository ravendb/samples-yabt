import { Component } from '@angular/core';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { ActivatedRoute, Router } from '@angular/router';
import { CustomFieldListGetRequest, CustomFieldListGetResponse } from '@core/api-models/custom-field/list';
import { CustomFieldsService } from '@core/api-services/customfields.service';
import { ListBaseComponent } from '@core/base-list/list-base.component';
import { filter } from 'rxjs/operators';
import { CustomFieldDialogComponent, IDialogData } from '../item';

@Component({
	styleUrls: ['./custom-field-list.component.scss'],
	templateUrl: './custom-field-list.component.html',
})
export class CustomFieldListComponent extends ListBaseComponent<CustomFieldListGetResponse, CustomFieldListGetRequest> {
	constructor(router: Router, activatedRoute: ActivatedRoute, apiService: CustomFieldsService, private dialog: MatDialog) {
		super(router, activatedRoute, apiService, ['name', 'fieldType', 'backlogItemTypes'], {});
	}

	openItemDialog(id?: string): void {
		let params: MatDialogConfig = { minWidth: '350px' };
		this.subscriptions.add(
			this.dialog
				.open(CustomFieldDialogComponent, !!id ? { ...params, data: { id } as IDialogData } : params)
				.afterClosed()
				.pipe(filter(Boolean))
				.subscribe(() => this.refreshList())
		);
	}
}
