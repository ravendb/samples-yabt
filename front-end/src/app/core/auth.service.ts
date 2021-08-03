import { Injectable } from '@angular/core';
import { first } from 'lodash-es';
import { UsersConfigModel } from './api-models/users-config.model';
import { AppConfigService } from './app-config.service';

@Injectable({
	providedIn: 'root',
})
export class AuthService {
	private currentUserId: string | undefined;

	constructor(private appCfgService: AppConfigService) {}

	setCurrentUser(userId: string | undefined): void {
		this.currentUserId = userId;
	}

	getCurrentUser(): UsersConfigModel | undefined {
		const usersAndTenants = this.appCfgService.getConfiguredUsersAndTenants();
		return !!this.currentUserId ? usersAndTenants.find(u => u.userId == this.currentUserId) : first(usersAndTenants);
	}
}
