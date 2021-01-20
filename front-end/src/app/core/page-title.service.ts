import { Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs/operators';

@Injectable({
	providedIn: 'root',
})
export class PageTitleService {
	title: string = '';

	constructor(private router: Router, private titleService: Title, private activatedRoute: ActivatedRoute) {}

	initializePageTitles() {
		this.getTitleFromRoute();
		this.router.events.pipe(filter(event => event instanceof NavigationEnd)).subscribe((): void => {
			this.getTitleFromRoute();
		});
	}

	private getTitleFromRoute() {
		let root = this.activatedRoute.snapshot.root;

		while (root.children && root.children.length) {
			root.children.forEach(childRoute => {
				root = childRoute;
				this.title = childRoute?.data?.title || this.title;
			});
		}

		if (this.title && this.title.indexOf('{{') === -1) this.titleService.setTitle(this.title);
	}
}
