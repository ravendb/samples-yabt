import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { AppConfigService } from '@core/app-config.service';
import { SessionStorageService } from '@core/storage.service';
import { filter, take } from 'rxjs/operators';
import { ChangeUserAndTenantDialogComponent } from '.';

@Injectable({
	providedIn: 'root',
})
export class ChangeUserAndTenantService {
	constructor(private dialogService: MatDialog, private sessionStorageService: SessionStorageService) {}

	displayDialog(): void {
		this.dialogService
			.open(ChangeUserAndTenantDialogComponent, { width: '400px' })
			.beforeClosed()
			.pipe(
				filter(s => !!s), // Continue only on the "OK" button
				take(1)
			)
			.subscribe((key: string) => {
				this.sessionStorageService.setItem(AppConfigService.sessionUserChangedParamName, key);
				location.href = '/';
			});
	}
}
