import { Component, OnInit } from '@angular/core';
import { IBreadcrumbItem, PageTitleService } from '@core/page-title.service';

@Component({
	selector: 'app-breadcrumbs',
	templateUrl: './breadcrumbs.component.html',
	styleUrls: ['./breadcrumbs.component.scss'],
})
export class BreadcrumbsComponent implements OnInit {
	get breadcrumbs(): IBreadcrumbItem[] {
		return this.pageTitle.breadcrumbs;
	}

	constructor(private pageTitle: PageTitleService) {}

	ngOnInit(): void {}
}
