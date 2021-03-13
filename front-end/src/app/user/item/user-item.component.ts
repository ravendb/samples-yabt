import { Location } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { UserAddUpdRequest, UserGetByIdResponse } from '@core/api-models/user/item';
import { UsersService } from '@core/api-services/users.service';
import { INotificationMessage } from '@core/notification';
import { NotificationService } from '@core/notification/notification.service';
import { CustomValidators } from '@utils/custom-validators';
import { of, Subscription } from 'rxjs';
import { filter, switchMap, take } from 'rxjs/operators';

@Component({
	templateUrl: './user-item.component.html',
	styleUrls: ['./user-item.component.scss'],
})
export class UserItemComponent implements OnInit, OnDestroy {
	editId: string | null = null;
	form!: FormGroupTyped<UserAddUpdRequest>;

	private subscriptions = new Subscription();
	private _userDtoBeforeUpdate: UserGetByIdResponse | undefined;
	private _listRoute = '/users';

	constructor(
		private activatedRoute: ActivatedRoute,
		private router: Router,
		private fb: FormBuilder,
		private apiService: UsersService,
		private notifyService: NotificationService,
		private location: Location
	) {}

	ngOnInit(): void {
		this.form = this.fb.group({
			firstName: [null, [CustomValidators.requiredWhen(() => !this.form?.controls.lastName.value, 'First name')]],
			lastName: [null, [CustomValidators.requiredWhen(() => !this.form?.controls.firstName.value, 'Last name')]],
			avatarUrl: [null, [CustomValidators.optionalUrl()]],
			email: [null, [CustomValidators.email()]],
		}) as FormGroupTyped<UserAddUpdRequest>;

		this.subscriptions.add(
			this.activatedRoute.paramMap
				.pipe(
					switchMap((p: ParamMap) => {
						const id = p.get('id');
						this.editId = !!id && id !== 'create' ? id : null;
						return !!this.editId ? this.apiService.getUser(this.editId) : of({} as UserGetByIdResponse);
					})
				)
				.subscribe(user => {
					this._userDtoBeforeUpdate = user;
					this.form.reset(user);
				})
		);
	}
	ngOnDestroy() {
		this.subscriptions.unsubscribe();
	}

	save(): void {
		var saveCmd = !!this.editId
			? this.apiService.updateUser(this.editId, this.form.value)
			: this.apiService.createUser(this.form.value);
		saveCmd.pipe(take(1)).subscribe(
			ref => {
				const notification = {
					linkRoute: [this._listRoute, ref.id],
					linkText: ref.name,
					text: 'User saved:',
				} as INotificationMessage;
				this.notifyService.showNotificationWithLink(notification);
				this.goBack();
			},
			err => {
				this.notifyService.showError('Failed to save', 'Invalid form values.', err);
			}
		);
	}

	delete(): void {
		if (!this.editId) return;

		this.notifyService
			.showDeleteConfirmation('Delete Custom Field?', `Do you want delete '<b>${this._userDtoBeforeUpdate?.nameWithInitials}</b>'?`)
			.pipe(
				filter(r => r),
				switchMap(() => this.apiService.deleteUser(this.editId!)),
				take(1)
			)
			.subscribe(
				ref => {
					this.notifyService.showNotification(`User '${ref.name}' deleted`);
					this.goBack();
				},
				err => {
					this.notifyService.showError('Failed to delete', 'Deleting user failed:', err);
				}
			);
	}

	goBack(): void {
		if (window.history.length > 1) this.location.back();
		else this.router.navigate([this._listRoute]);
	}
}
