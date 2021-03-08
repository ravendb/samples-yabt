import { Injectable } from '@angular/core';

@Injectable({
	providedIn: 'root',
})
export class AppConfigService {
	private readonly cookieNameApiBaseUrl = 'apiBaseUrl';
	private readonly cookieNameApiUserKeys = 'apiUserKeys';

	private readonly pageSizes = [10, 15, 20, 30, 50, 100];

	constructor() {}

	getAppServerUrl(): string {
		return this.getCookie(this.cookieNameApiBaseUrl, 'https://localhost:5001');
	}

	getCurrentApiKey(): string {
		return this.getCookie(this.cookieNameApiUserKeys, '4D84AE02-C989-4DC5-9518-8D0CB2FB5F61');
	}

	getPageSizeOptions(): number[] {
		return this.pageSizes;
	}

	getPageSize(): number {
		return this.pageSizes[1];
	}

	private getCookie(name: string, defaultValue: string): string {
		const value = ('; ' + document.cookie).split(`; ${name}=`).pop()?.split(';').shift();
		return !!value ? decodeURIComponent(value) : defaultValue;
	}
}
