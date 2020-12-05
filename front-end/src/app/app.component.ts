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
			<div class="main-outlet">
				<router-outlet></router-outlet>
			</div>
			<app-footer></app-footer>
		</main-menu>
	`,
})
export class AppComponent implements OnInit, OnDestroy {
	private subscriptions: Subscription = new Subscription();

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
