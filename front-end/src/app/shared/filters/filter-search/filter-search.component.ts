import { Component, OnDestroy, OnInit } from '@angular/core';
import { ControlContainer, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CustomFormControlBase } from '@shared/forms/custom-form-control-base';
import { Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
	selector: 'filter-search',
	styleUrls: ['./filter-search.component.scss'],
	templateUrl: './filter-search.component.html',
	providers: [
		{
			provide: NG_VALUE_ACCESSOR,
			useExisting: FilterSearchComponent,
			multi: true,
		},
	],
})
export class FilterSearchComponent extends CustomFormControlBase<string> implements OnInit, OnDestroy {
	private _subscriptions: Subscription = new Subscription();

	constructor(controlContainer: ControlContainer) {
		super(controlContainer);
	}

	ngOnInit() {
		this._subscriptions.add(
			this.mainControl.valueChanges.pipe(distinctUntilChanged(), debounceTime(300)).subscribe((val: string) => this._onChange(val))
		);
	}
	ngOnDestroy() {
		this._subscriptions.unsubscribe();
	}
}
