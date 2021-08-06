import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { LocalStorageService } from '@core/storage.service';
import { filter, take } from 'rxjs/operators';
import { WelcomeDialogComponent } from './welcome-dialog.component';

@Injectable({
	providedIn: 'root',
})
export class WelcomeDialogCheckService {
	readonly storageTimestampKey = 'welcomeDismissedAt';
	readonly minDaysForShowingWelcomeAgain = 7;

	constructor(private dialogService: MatDialog, private storageService: LocalStorageService) {}

	displayWelcomeDialogIfNecessary(): void {
		let showWelcome = true;
		const storageTimestampStr = this.storageService.getItem(this.storageTimestampKey);
		if (!!storageTimestampStr) {
			const storageTimestamp = new Date(storageTimestampStr);
			showWelcome = this.diffFromNowInDays(storageTimestamp) >= this.minDaysForShowingWelcomeAgain;
		}

		if (showWelcome) {
			this.dialogService
				.open(WelcomeDialogComponent, { width: '600px' })
				.beforeClosed()
				.pipe(
					filter(Boolean), // Continue only on the "OK" button
					take(1)
				)
				.subscribe(_ =>
					// Save when the user dismissed the prompt
					this.storageService.setItem(this.storageTimestampKey, new Date().toISOString())
				);
		}
	}

	private diffFromNowInDays(dateTime: Date): number {
		const diffTime = Math.abs(new Date().getTime() - dateTime.getTime());
		return Math.ceil(diffTime / (1000 * 3600 * 24));
	}
}
