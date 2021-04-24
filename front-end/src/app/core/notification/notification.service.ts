import { Injectable, NgZone } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';
import { HttpErrorDetails } from '@core/api-services/http-error-details';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { AlertDialogComponent } from './alert-dialog.component';
import { ConfirmationDialogComponent } from './confirmation-dialog.component';
import { INotificationMessage, NotificationComponent } from './notification.component';

@Injectable({
	providedIn: 'root',
})
export class NotificationService {
	constructor(private dialog: MatDialog, private notifySnack: MatSnackBar, private zone: NgZone) {}

	showNotification(text: string) {
		this.showNotificationWithLink({ text });
	}

	showNotificationWithLink(data: INotificationMessage, duration: number = 3000) {
		this.showNotificationWithLinks([data], duration);
	}
	private showNotificationWithLinks(data: INotificationMessage[], duration: number = 3000) {
		const config = new MatSnackBarConfig<INotificationMessage[]>();
		config.duration = duration;
		config.data = data;
		this.zone.run(() => {
			this.notifySnack.openFromComponent(NotificationComponent, config);
		});
	}

	showError(title: string, htmlBody: string, errDetails: HttpErrorDetails | undefined = undefined): Observable<void> {
		if (!!errDetails) {
			let addedMsg =
				(!!errDetails.title ? '<br>' + errDetails.title : '') +
				(!!errDetails.detail ? '<br>' + errDetails.detail.replace('\n', '<br>\n') : '');
			if (!addedMsg) addedMsg = '' + errDetails;
			htmlBody += addedMsg;
		}
		const dialogRef = this.dialog.open(AlertDialogComponent, {
			data: {
				html: htmlBody,
				title,
			},
		});
		return dialogRef.afterClosed().pipe(take(1));
	}

	/* Show a Yes/Cancel dialog */
	showConfirmation(title: string, html: string, confirmationText: string = '', cancellationText: string = ''): Observable<boolean> {
		const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
			data: {
				cancellationText,
				confirmationText,
				html,
				title,
			},
		});
		return dialogRef.afterClosed().pipe(take(1));
	}

	/* Show a Yes/Cancel dialog with a delete verification feature */
	showDeleteConfirmation(
		title: string,
		html: string,
		confirmationText: string = '',
		cancellationText: string = '',
		deleteValidationText: string = '',
		deleteValidationValue: string = '',
		deleteValidationPlaceHolder: string = ''
	): Observable<boolean> {
		const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
			data: {
				cancellationText,
				confirmationText,
				deleteValidationPlaceHolder,
				deleteValidationText,
				deleteValidationValue,
				html,
				title,
			},
		});
		return dialogRef.afterClosed().pipe(take(1));
	}
}
