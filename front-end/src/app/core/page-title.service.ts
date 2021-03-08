import { Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { isEmpty, last } from 'lodash-es';

export interface IBreadcrumbItem {
	label: string;
	url: string;
}
@Injectable({
	providedIn: 'root',
})
export class PageTitleService {
	get fullPageTitle(): string {
		return (!isEmpty(this._title) ? this._title + ' | ' : '') + 'YABT';
	}
	private _title = '';

	get breadcrumbs(): IBreadcrumbItem[] {
		return this._breadcrumbs;
	}
	private _breadcrumbs: IBreadcrumbItem[] = [];

	constructor(private titleService: Title, private activatedRoute: ActivatedRoute) {}

	initializePageTitles(): void {
		this._breadcrumbs = this.getNestedRoutes(this.activatedRoute.root);
		this._title = last(this._breadcrumbs)?.label || '';

		this.titleService.setTitle(this.fullPageTitle);
	}
	addLastBreadcrumbs(item: IBreadcrumbItem): void {
		this._breadcrumbs.push(item);
	}
	setLastBreadcrumbs(item: IBreadcrumbItem): void {
		if (!this._breadcrumbs || this._breadcrumbs.length < 1) return;
		this._breadcrumbs[this._breadcrumbs.length - 1] = item;
	}

	private getNestedRoutes(route: ActivatedRoute, url: string = '', menuItems: IBreadcrumbItem[] = []): IBreadcrumbItem[] {
		if (!isEmpty(route.children))
			route.children.forEach(child => {
				const routeURL: string = child.snapshot.url.map(segment => segment.path).join('/');
				if (isEmpty(routeURL)) return menuItems;
				else {
					url += `/${routeURL}`;
					const label = child.snapshot?.data?.title;
					if (!!label) {
						menuItems.push({ label, url });
					}

					return this.getNestedRoutes(child, url, menuItems);
				}
			});
		return menuItems;
	}
}
