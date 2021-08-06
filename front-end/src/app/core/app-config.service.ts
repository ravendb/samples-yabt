import { Injectable } from '@angular/core';
import { attempt, isError } from 'lodash-es';
import { UsersConfigModel } from './api-models/users-config.model';

@Injectable({
	providedIn: 'root',
})
export class AppConfigService {
	public static readonly sessionUserChangedParamName = 'newUser';

	private readonly cookieNameApiBaseUrl = 'apiBaseUrl';
	private readonly cookieNameApiUserKeys = 'apiUsers';

	private readonly pageSizes = [10, 15, 20, 30, 50, 100];

	private cachedApiBaseUrl: string | null = null;
	private cachedApiUserKeys: UsersConfigModel[] | null = null;

	getAppServerUrl(): string {
		if (!this.cachedApiBaseUrl) this.cachedApiBaseUrl = this.getCookie(this.cookieNameApiBaseUrl, 'https://localhost:5001');
		return this.cachedApiBaseUrl;
	}

	getConfiguredUsersAndTenants(): UsersConfigModel[] {
		if (!this.cachedApiUserKeys) {
			// The hard-coded value is used exclusively for dev purposes
			const defaultValue = [
				<UsersConfigModel>{
					apiKey: '4C81D6EC-AA5A-4799-8538-84E31CF5493B',
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

	/*
		Get value from a cookie.
			`T` - type of the expected object (attempts to parse a serialised JSON object)
			`name` - name of the cookie
			`defaultValue` - returned default values if the specified cookie not found
	*/
	private getCookie<T>(name: string, defaultValue: T): T {
		const value = ('; ' + document.cookie).split(`; ${name}=`).pop()?.split(';').shift();
		if (!!value) {
			const decodedVal = decodeURIComponent(value);
			// The string value might be an object that can be deserialized
			const parseResult = attempt(JSON.parse.bind(null, decodedVal));
			return !isError(parseResult) ? parseResult : decodedVal;
		}
		return defaultValue;
	}
}
