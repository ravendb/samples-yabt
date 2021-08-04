import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ChangeUserAndTenantDialogComponent } from '.';

@Injectable({
	providedIn: 'root',
})
export class ChangeUserAndTenantService {
	constructor(private dialogService: MatDialog) {}

	displayDialog(): void {
		this.dialogService.open(ChangeUserAndTenantDialogComponent, { width: '400px' });
	}
}
