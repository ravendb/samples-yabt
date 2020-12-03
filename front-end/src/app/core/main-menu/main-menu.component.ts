import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { AfterViewInit, ChangeDetectorRef, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';
import { Router } from '@angular/router';
import { get, some } from 'lodash-es';
import { Subscription } from 'rxjs';
import { MainMenuMapItem } from './main-menu-map-item';

/* Side menu (aka 'Main menu') */
@Component({
	selector: 'main-menu',
	styleUrls: ['./main-menu.component.scss'],
	templateUrl: 'main-menu.component.html',
})
export class MainMenuComponent implements OnInit, AfterViewInit, OnDestroy {
	@ViewChild(MatSidenav)
	sidenav!: MatSidenav;

	// Items of the side menu
	siteMap: MainMenuMapItem[] = [
		new MainMenuMapItem('Backlog Items', '/backlog', ['/backlog'], 'subject'),
		new MainMenuMapItem('Users', '/users', ['/users'], 'supervisor_account'),
	];

	isMobile: boolean = false;

	userId = 0;
	userName = 'Test User';

	// Flag: whether the side menu is collapsed
	private toggleCollapsed: boolean = true;
	private subscriptions: Subscription = new Subscription();

	constructor(private breakpoint: BreakpointObserver, private router: Router, private cdr: ChangeDetectorRef) {}

	ngOnInit(): void {}

	ngAfterViewInit(): void {
		this.subscriptions.add(
			this.breakpoint.observe([Breakpoints.Handset]).subscribe(result => {
				this.isMobile = result.matches;
				this.sidenav.mode = this.isMobile ? 'over' : 'side';
				// When the sidenav is closed it's not visible at all. 'open' means displayed down the left hand edge,
				// regardless of whether it's expanded to show the text labels. Thus:
				// When switching to mobile view make sure the panel is closed (will open on clicking the menu)
				// When switching into desktop view make sure the panel is open and visible (although not necessarily
				// expanded.) If we didn't do this there'd be no way to open/view it in desktop layout.
				if (this.isMobile && this.sidenav.opened) {
					this.sidenav.close();
				} else if (!this.isMobile && !this.sidenav.opened) {
					this.sidenav.open();
				}

				this.cdr.detectChanges();
			})
		);
	}

	ngOnDestroy(): void {
		this.subscriptions.unsubscribe();
	}

	// Collapse/expand the menu
	expandCollapseSideMenu(): void {
		this.toggleCollapsed = !this.toggleCollapsed;
	}

	isMenuButtonHighlighted(item: MainMenuMapItem): boolean {
		return some(item.highlightedLinks, r => this.router.url.split('?')[0] === r);
	}

	openMenu(): void {
		this.sidenav.open();
	}

	closeMenu(): void {
		this.toggleCollapsed = true;
		if (this.isMobile) {
			this.sidenav.close();
		}
	}

	openedRecentItemsMenu(): void {}

	get isCollapsed(): boolean {
		return !this.isExpanded;
	}

	get isExpanded(): boolean {
		return !this.toggleCollapsed || this.isMobile;
	}

	get mobileTitle(): string {
		return get(
			this.siteMap.find(item => this.router.url.startsWith(item.uriStartsWith)),
			'title',
			'System'
		);
	}
}
