import { Injectable } from '@angular/core';
import { UsersConfigModel } from './api-models/users-config.model';
import { AppConfigService } from './app-config.service';
import { SessionStorageService } from './storage.service';

@Injectable({
	providedIn: 'root',
})
export class AuthService {
	private currentUserApiKey: string | undefined;

	constructor(private appCfgService: AppConfigService, private storageService: SessionStorageService) {}

	getCurrentUser(): UsersConfigModel {
		const usersAndTenants = this.appCfgService.getConfiguredUsersAndTenants();
		if (!this.currentUserApiKey) {
			var newUserFromSession = this.storageService.getItem(AppConfigService.sessionUserChangedParamName);
			this.storageService.removeItem(AppConfigService.sessionUserChangedParamName);
			if (!!newUserFromSession) this.currentUserApiKey = newUserFromSession;
		}
		if (!!this.currentUserApiKey) {
			const u = usersAndTenants.find(u => u.apiKey == this.currentUserApiKey);
			if (!u) throw new Error('Failed to resolve allowed user for ' + this.currentUserApiKey);
			return u;
		} else return usersAndTenants[0];
	}
}
