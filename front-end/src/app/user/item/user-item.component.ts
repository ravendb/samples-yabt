import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { UserAddUpdRequest } from '@core/api-models/user/item';
import { UsersService } from '@core/api-services/users.service';
import { CustomValidators } from '@utils/custom-validators';
import { of, Subscription } from 'rxjs';
import { switchMap, take } from 'rxjs/operators';

@Component({
	templateUrl: './user-item.component.html',
	styleUrls: ['./user-item.component.scss'],
})
export class UserItemComponent implements OnInit, OnDestroy {
	editId: string | null = null;
	form!: FormGroupTyped<UserAddUpdRequest>;

	private subscriptions = new Subscription();

	constructor(private activatedRoute: ActivatedRoute, private fb: FormBuilder, private apiService: UsersService) {}

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
						return !!this.editId ? this.apiService.getUser(this.editId) : of({});
					})
				)
				.subscribe(user => this.form.reset(user))
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
			ref => {},
			err => {}
		);
	}
	delete(): void {
		if (!!this.editId)
			this.apiService
				.deleteUser(this.editId)
				.pipe(take(1))
				.subscribe(
					ref => {},
					err => {}
				);
	}
}
