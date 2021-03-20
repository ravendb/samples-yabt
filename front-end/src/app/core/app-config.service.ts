import { Injectable } from '@angular/core';

@Injectable({
	providedIn: 'root',
})
export class AppConfigService {
	private readonly cookieNameApiBaseUrl = 'apiBaseUrl';
	private readonly cookieNameApiUserKeys = 'apiUserKeys';

	private readonly pageSizes = [10, 15, 20, 30, 50, 100];

	private cachedApiBaseUrl: string | null = null;
	private cachedApiUserKeys: string | null = null;

	constructor() {}

	getAppServerUrl(): string {
		if (!this.cachedApiBaseUrl) this.cachedApiBaseUrl = this.getCookie(this.cookieNameApiBaseUrl, 'https://localhost:5001');
		return this.cachedApiBaseUrl;
	}

	getCurrentApiKey(): string {
		if (!this.cachedApiUserKeys)
			this.cachedApiUserKeys = this.getCookie(this.cookieNameApiUserKeys, '4D84AE02-C989-4DC5-9518-8D0CB2FB5F61');
		return this.cachedApiUserKeys;
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
