import { CommonModule } from '@angular/common';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ModuleWithProviders, NgModule, Optional, SkipSelf } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatIconModule, MatIconRegistry } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatRadioModule } from '@angular/material/radio';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { DomSanitizer } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { AuthInterceptor } from './auth.interceptor';
import { ChangeUserAndTenantDialogComponent } from './change-user-and-tenant-dialog';
import { FooterComponent } from './footer';
import { MainMenuComponent } from './main-menu';
import { BreadcrumbsComponent } from './main-menu/breadcrumbs/breadcrumbs.component';
import { ConfirmationDialogComponent } from './notification';
import { AlertDialogComponent } from './notification/alert-dialog.component';
import { NotificationComponent } from './notification/notification.component';
import { NotificationService } from './notification/notification.service';
import { WelcomeDialogComponent } from './welcome-dialog';

@NgModule({
	declarations: [
		AlertDialogComponent,
		FooterComponent,
		MainMenuComponent,
		NotificationComponent,
		ConfirmationDialogComponent,
		BreadcrumbsComponent,
		ChangeUserAndTenantDialogComponent,
		WelcomeDialogComponent,
	],
	exports: [FooterComponent, MainMenuComponent],
	imports: [
		CommonModule,
		FormsModule,
		HttpClientModule,
		MatButtonModule,
		MatDialogModule,
		MatIconModule,
		MatListModule,
		MatMenuModule,
		MatRadioModule,
		MatSidenavModule,
		MatSnackBarModule,
		MatToolbarModule,
		MatTooltipModule,
		RouterModule,
	],
	providers: [
		{
			multi: true,
			provide: HTTP_INTERCEPTORS,
			useClass: AuthInterceptor,
		},
	],
})
export class CoreModule {
	// The core module must be imported only in the root module. Other modules must not import the core modules.
	// This c-tor is a guard to prevent double import
	// See https://angular.io/guide/singleton-services#prevent-reimport-of-the-greetingmodule
	constructor(@Optional() @SkipSelf() core: CoreModule, iconRegistry: MatIconRegistry, sanitizer: DomSanitizer) {
		if (core) {
			throw new Error('You should import core module only in the root module');
		}
		iconRegistry.addSvgIcon('GitHub', sanitizer.bypassSecurityTrustResourceUrl('/assets/img/github.svg'));
	}

	// Use forRoot() to import a module with services, which need to be SINGLETON for lazy-loading modules.
	// Otherwise, the lazy modules will have new instances of services created for them
	// See https://angular.io/guide/singleton-services#the-forroot-pattern
	static forRoot(): ModuleWithProviders<CoreModule> {
		return {
			ngModule: CoreModule,
			providers: [NotificationService],
		};
	}
}
