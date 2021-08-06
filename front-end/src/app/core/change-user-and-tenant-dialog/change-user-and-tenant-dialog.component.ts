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

	currentUser: string;

	constructor(appCfgService: AppConfigService, authService: AuthService) {
		this.users = appCfgService.getConfiguredUsersAndTenants();
		this.currentUser = authService.getCurrentUser().apiKey;
	}
}
