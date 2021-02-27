import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { empty, Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AppConfig } from './app.config';
import { NotificationService } from './notification/notification.service';

/**
 * Adds the authorisation headers and handles 401,403 codes
 */
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
	private apiKey = AppConfig.ApiKey;

	private noInternetMessageShown = false;
	private authFailedRequestShown = false;

	constructor(private notifyService: NotificationService) {}

	intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		const updatedRequest = req.clone({
			setHeaders: { 'X-API-Key': `${this.apiKey}` },
			withCredentials: true,
		});
		return next.handle(updatedRequest).pipe(
			catchError(err => {
				// Catch the 401/403 status code to redirect to the login page
				if (err instanceof HttpErrorResponse && [401, 403].indexOf(err.status) > -1) {
					if (!this.authFailedRequestShown) {
						this.authFailedRequestShown = true;
						// No 'switchMap()' here and start a new flow via 'setTimeout()', as flipping HttpClient pipeline
						// to UI can be troublesome. The HttpClient pipeline need to finish as it's expected
						setTimeout(() =>
							this.notifyService
								.showError('Authentication failed', 'Please report it as an issue on GitHub')
								.subscribe(() => (this.authFailedRequestShown = false))
						);
					}
					// Don't throw an error if the notification is already shown to the user
					return empty();
				}
				// If the internet connection is lost
				// * status code would be 0
				// * window.navigator.onLine would return false
				else if (err instanceof HttpErrorResponse && err.status === 0 && !window.navigator.onLine) {
					if (!this.noInternetMessageShown) {
						this.noInternetMessageShown = true;
						setTimeout(() =>
							this.notifyService
								.showConfirmation(
									'Internet Connection Lost',
									'Unable to connect to Internet. Please check your Internet connection.',
									'Reload',
									'Cancel'
								)
								.subscribe((isRefresh: boolean) => {
									if (isRefresh) {
										window.location.reload();
									} else {
										this.noInternetMessageShown = false;
									}
								})
						);
					}
					// Don't throw an error if the notification is already shown to the user
					return empty();
				}
				return throwError(err);
			})
		);
	}
}