import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { EMPTY, Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { NotificationService } from './notification/notification.service';

/**
 * Adds the authorisation headers and handles 401,403 codes
 */
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
	private noInternetMessageShown = false;
	private authFailedRequestShown = false;

	constructor(private notifyService: NotificationService, private authService: AuthService) {}

	intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		const apiKey = this.authService.getCurrentUser().apiKey;
		if (!apiKey) throw new Error('Failed to resolve the API key');

		const updatedRequest = req.clone({
			setHeaders: { 'X-API-Key': `${apiKey}` },
			withCredentials: true,
		});
		return next.handle(updatedRequest).pipe(
			catchError(err => {
				// Catch the 401/403 status code to redirect to the login page
				if (err instanceof HttpErrorResponse && [401, 403].indexOf(err.status) > -1) {
					if (!this.authFailedRequestShown) {
						this.authFailedRequestShown = true;
						const message =
							(!!err?.error?.detail ? err!.error!.detail + '<br><br>\n' : '') +
							'Please report it as an issue on GitHub (<a href="https://github.com/ravendb/samples-yabt/issues" target="_blank">link</a>)';

						// No 'switchMap()' here and start a new flow via 'setTimeout()', as flipping HttpClient pipeline
						// to UI can be troublesome. The HttpClient pipeline need to finish as it's expected
						setTimeout(() =>
							this.notifyService
								.showError('Authentication failed', message)
								.subscribe(() => (this.authFailedRequestShown = false))
						);
					}
					// Don't throw an error if the notification is already shown to the user
					return EMPTY;
				}
				// If server can't be accessed
				// * status code would be 0
				else if (err instanceof HttpErrorResponse && err.status === 0) {
					if (!this.noInternetMessageShown) {
						this.noInternetMessageShown = true;
						setTimeout(() => {
							// If the internet connection is lost
							// * window.navigator.onLine would return false
							const title = !window.navigator.onLine ? 'Internet Connection Lost' : 'No server connection';
							const msg = !window.navigator.onLine
								? 'Unable to connect to Internet. Please check your Internet connection.'
								: 'Unable to connect to the server while Internet connection is present.';
							return this.notifyService.showConfirmation(title, msg, 'Reload', 'Cancel').subscribe((isRefresh: boolean) => {
								if (isRefresh) {
									window.location.reload();
								} else {
									this.noInternetMessageShown = false;
								}
							});
						});
					}
					// Don't throw an error if the notification is already shown to the user
					return EMPTY;
				}
				return throwError(err);
			})
		);
	}
}
