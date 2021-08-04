import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { AfterViewInit, ChangeDetectorRef, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';
import { Router } from '@angular/router';
import { CurrentUserResponse } from '@core/api-models/user/item/CurrentUserResponse';
import { UsersService } from '@core/api-services/users.service';
import { ChangeUserAndTenantService } from '@core/change-user-and-tenant-dialog/change-user-and-tenant.service';
import { get, some } from 'lodash-es';
import { Subscription } from 'rxjs';
import { MainMenuMapItem } from './main-menu-map-item';

/*
	It's the Main Menu. 
	Most of the logic here is to workaround for an infamous Material problem - lacking of a mini/narrow version of the Drawer/SideNav
	(the Wailing Wall - https://github.com/angular/components/issues/1728).
 */
@Component({
	selector: 'app-main-menu',
	styleUrls: ['./main-menu.component.scss'],
	templateUrl: 'main-menu.component.html',
})
export class MainMenuComponent implements OnInit, AfterViewInit, OnDestroy {
	@ViewChild(MatSidenav)
	private sidenav!: MatSidenav;

	// Items of the side menu
	readonly siteMap: MainMenuMapItem[] = [
		new MainMenuMapItem('Backlog Items', '/backlog-items', ['/backlog-items'], 'bug_report'),
		new MainMenuMapItem('Users', '/users', ['/users'], 'supervisor_account'),
	];

	get currentUser(): CurrentUserResponse | undefined {
		return this._currentUser;
	}
	get currentUserLink(): string | undefined {
		return !!this.currentUser?.id ? `/users/${this.currentUser.id}` : '';
	}
	get isMobile() {
		return this._isMobile;
	}

	get isNarrowMenu() {
		return !this._isMobile && this._isNarrowMenu;
	}
	get mobileTitle(): string {
		return get(
			this.siteMap.find(item => this.router.url.startsWith(item.uriStartsWith)),
			'title',
			'System'
		);
	}

	private _isMobile: boolean = false;
	// Flag: whether the side menu is collapsed
	private _isNarrowMenu: boolean = true;
	private _currentUser: CurrentUserResponse | undefined;
	private _subscriptions: Subscription = new Subscription();

	constructor(
		private breakpoint: BreakpointObserver,
		private router: Router,
		private cdr: ChangeDetectorRef,
		private userService: UsersService,
		private changeUserDialogService: ChangeUserAndTenantService
	) {}

	ngOnInit(): void {
		this._subscriptions.add(this.userService.getCurrentUser().subscribe(r => (this._currentUser = r)));
	}

	ngAfterViewInit(): void {
		this._subscriptions.add(
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
		this._subscriptions.unsubscribe();
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

	changeUserAndTenantDialog(): void {
		this.changeUserDialogService.displayDialog();
	}

	openedRecentItemsMenu(): void {}
}
