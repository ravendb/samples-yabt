import { DOCUMENT } from '@angular/common';
import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RoutesRecognized } from '@angular/router';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';

@Component({
	selector: 'app-root',
	styleUrls: ['./app.component.scss'],
	template: `
		<main-menu>
			<mat-sidenav-container>
				<mat-sidenav position="end" mode="side" [opened]="showPreview">
					<router-outlet name="preview"></router-outlet>
				</mat-sidenav>
				<mat-sidenav-content>
					<router-outlet></router-outlet>
				</mat-sidenav-content>
			</mat-sidenav-container>
			<app-footer></app-footer>
		</main-menu>
	`,
})
export class AppComponent implements OnInit, OnDestroy {
	private subscriptions: Subscription = new Subscription();

	get showPreview(): boolean {
		return this.activatedRoute.snapshot.children.find(child => child.outlet === 'preview') != null;
	}
	constructor(private router: Router, private activatedRoute: ActivatedRoute, @Inject(DOCUMENT) private document: Document) {}

	ngOnInit() {
		this.subscriptions.add(
			this.router.events.pipe(filter(event => event instanceof RoutesRecognized)).subscribe((): void => {
				// Scroll to top on Route Change (still doesn't work on iOS though)
				// taken from http://stackoverflow.com/a/39601987/968003
				this.document.body.scrollTop = 0;
			})
		);
	}

	ngOnDestroy() {
		this.subscriptions.unsubscribe();
	}
}
