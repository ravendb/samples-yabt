import { Component } from '@angular/core';
import { UsersConfigModel } from '@core/api-models/users-config.model';
import { AppConfigService } from '@core/app-config.service';
import { AuthService } from '@core/auth.service';

@Component({
	styleUrls: ['./change-user-and-tenant-dialog.component.scss'],
	templateUrl: 'change-user-and-tenant-dialog.component.html',
})
export class ChangeUserAndTenantDialogComponent {
	users: UsersConfigModel[];

	get currentUserId(): string {
		return this.authService.getCurrentUser().userId;
	}
	set currentUserId(value: string) {
		this.authService.setCurrentUser(value);
	}

	constructor(appCfgService: AppConfigService, private authService: AuthService) {
		this.users = appCfgService.getConfiguredUsersAndTenants();
	}
}
