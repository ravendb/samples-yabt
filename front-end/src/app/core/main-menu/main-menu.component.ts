import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { AfterViewInit, ChangeDetectorRef, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';
import { Router } from '@angular/router';
import { get, some } from 'lodash-es';
import { Subscription } from 'rxjs';
import { MainMenuMapItem } from './main-menu-map-item';

/*
	It's the Main Menu. 
	Most of the logic here is to workaround for an infamous Material problem - lacking of a mini/narrow version of the Drawer/SideNav
	(the Wailing Wall - https://github.com/angular/components/issues/1728).
 */
@Component({
	selector: 'main-menu',
	styleUrls: ['./main-menu.component.scss'],
	templateUrl: 'main-menu.component.html',
})
export class MainMenuComponent implements OnInit, AfterViewInit, OnDestroy {
	@ViewChild(MatSidenav)
	private sidenav!: MatSidenav;

	public get siteMap() {
		return this._siteMap;
	}
	public get isMobile() {
		return this._isMobile;
	}
	public get isNarrowMenu() {
		return !this._isMobile && this._isNarrowMenu;
	}
	get mobileTitle(): string {
		return get(
			this.siteMap.find(item => this.router.url.startsWith(item.uriStartsWith)),
			'title',
			'System'
		);
	}

	// TODO: Request user;s details from the back-end
	userId = 0;
	userName = 'Test User';

	// Items of the side menu
	private _siteMap: MainMenuMapItem[] = [
		new MainMenuMapItem('Backlog Items', '/backlog', ['/backlog'], 'bug_report'),
		new MainMenuMapItem('Users', '/user', ['/user'], 'supervisor_account'),
	];

	private _isMobile: boolean = false;
	// Flag: whether the side menu is collapsed
	private _isNarrowMenu: boolean = true;
	private subscriptions: Subscription = new Subscription();

	constructor(private breakpoint: BreakpointObserver, private router: Router, private cdr: ChangeDetectorRef) {}

	ngOnInit(): void {}

	ngAfterViewInit(): void {
		this.subscriptions.add(
			// It's very frustrating that this properties can't be controlled from CSS
			this.breakpoint.observe([Breakpoints.Handset]).subscribe(result => {
				this._isMobile = result.matches;
				this.sidenav.mode = this.isMobile ? 'over' : 'side';

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
		this._isNarrowMenu = !this._isNarrowMenu;
	}

	isMenuButtonHighlighted(item: MainMenuMapItem): boolean {
		return some(item.highlightedLinks, r => this.router.url.split('?')[0] === r);
	}

	expandMenu(): void {
		this._isNarrowMenu = false;
		this.sidenav.open();
	}

	collapseMenu(): void {
		if (this.isMobile) {
			this.sidenav.close();
		}
	}

	openedRecentItemsMenu(): void {}
}
