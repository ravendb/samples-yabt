import { DOCUMENT } from '@angular/common';
import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { PageTitleService } from '@core/page-title.service';
import { WelcomeDialogCheckService } from '@core/welcome-dialog/welcome-dialog-check.service';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';

@Component({
	selector: 'app-root',
	styleUrls: ['./app.component.scss'],
	template: `
		<app-main-menu>
			<div class="main-outlet">
				<router-outlet></router-outlet>
			</div>
			<app-footer></app-footer>
		</app-main-menu>
	`,
})
export class AppComponent implements OnInit, OnDestroy {
	private subscriptions: Subscription = new Subscription();

	constructor(
		private router: Router,
		@Inject(DOCUMENT) private document: Document,
		private pageTitle: PageTitleService,
		private welcomeDialogCheck: WelcomeDialogCheckService
	) {}

	async ngOnInit() {
		this.subscriptions.add(
			this.router.events.pipe(filter(event => event instanceof NavigationEnd)).subscribe((): void => {
				// Scroll to top on Route Change (still doesn't work on iOS though)
				// taken from http://stackoverflow.com/a/39601987/968003
				this.document.body.scrollTop = 0;
				// update the page title
				this.pageTitle.initializePageTitles();
			})
		);

		this.welcomeDialogCheck.displayWelcomeDialogIfNecessary();
	}

	ngOnDestroy() {
		this.subscriptions.unsubscribe();
	}
}
