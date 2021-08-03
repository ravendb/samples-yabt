import { Injectable } from '@angular/core';
import { attempt, isError } from 'lodash-es';
import { UsersConfigModel } from './api-models/users-config.model';

@Injectable({
	providedIn: 'root',
})
export class AppConfigService {
	private readonly cookieNameApiBaseUrl = 'apiBaseUrl';
	private readonly cookieNameApiUserKeys = 'apiUserKeys';

	private readonly pageSizes = [10, 15, 20, 30, 50, 100];

	private cachedApiBaseUrl: string | null = null;
	private cachedApiUserKeys: UsersConfigModel[] | null = null;

	getAppServerUrl(): string {
		if (!this.cachedApiBaseUrl) this.cachedApiBaseUrl = this.getCookie(this.cookieNameApiBaseUrl, 'https://localhost:5001');
		return this.cachedApiBaseUrl;
	}

	getConfiguredUsersAndTenants(): UsersConfigModel[] {
		if (!this.cachedApiUserKeys) {
			const defaultValue = [
				<UsersConfigModel>{
					userId: '4D84AE02-C989-4DC5-9518-8D0CB2FB5F61',
					userName: 'Test',
					tenantName: 'dotnet',
				},
			];

			this.cachedApiUserKeys = this.getCookie(this.cookieNameApiUserKeys, defaultValue);
		}
		return this.cachedApiUserKeys;
	}

	getPageSizeOptions(): number[] {
		return this.pageSizes;
	}

	getPageSize(): number {
		return this.pageSizes[1];
	}

	private getCookie<T>(name: string, defaultValue: T): T {
		const value = ('; ' + document.cookie).split(`; ${name}=`).pop()?.split(';').shift();
		if (!!value) {
			const decodedVal = decodeURIComponent(value);
			const parseResult = attempt(JSON.parse.bind(null, decodedVal));
			if (!isError(parseResult)) return parseResult;
		}
		return defaultValue;
	}
}
