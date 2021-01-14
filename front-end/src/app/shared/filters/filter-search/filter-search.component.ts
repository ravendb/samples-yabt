import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Subscription } from 'rxjs';
import { debounceTime, delay, distinctUntilChanged } from 'rxjs/operators';

@Component({
	selector: 'filter-search',
	styleUrls: ['./filter-search.component.scss'],
	templateUrl: './filter-search.component.html',
})
export class FilterSearchComponent implements OnInit, OnDestroy {
	@Input()
	control!: AbstractControl;

	searchControl: FormControlTyped<string> = new FormControl('');

	private _subscriptions: Subscription = new Subscription();

	ngOnInit() {
		if (!!this.control.value) this.searchControl.setValue(this.control.value);
		this._subscriptions.add(
			this.control.valueChanges
				.pipe(delay(0) /* Workaround for https://github.com/angular/components/issues/12070 */)
				.subscribe((val: string) => this.searchControl.setValue(val, { emitEvent: false }))
		);
		this._subscriptions.add(
			this.searchControl.valueChanges
				.pipe(distinctUntilChanged(), debounceTime(300))
				.subscribe((val: string) => this.control.setValue(val))
		);
	}

	ngOnDestroy() {
		this._subscriptions.unsubscribe();
	}
}
