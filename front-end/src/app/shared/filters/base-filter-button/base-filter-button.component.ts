import { Directive, Input, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { delay } from 'rxjs/operators';

@Directive()
export abstract class BaseFilterButtonComponent<TKey, TOption> implements OnInit, OnDestroy {
	@Input()
	get label(): string {
		return this._label;
	}
	set label(value: string) {
		this._label = value;
		this.updateLabel(this.control?.value);
	}
	@Input()
	buttonAltText: string = '';
	@Input()
	options: TOption[] = [];
	@Input()
	control!: AbstractControl;
	@Input()
	stretchedAndStroked: boolean = false;

	buttonText: string = this.label;

	protected _label: string = 'Unspecified';
	protected _subscriptions: Subscription = new Subscription();

	ngOnInit() {
		// Update the label on changing the selection
		this._subscriptions.add(this.control.valueChanges.pipe(delay(0)).subscribe((keys: TKey) => this.updateLabel(keys)));
	}
	ngOnDestroy() {
		this._subscriptions.unsubscribe();
	}

	protected abstract updateLabel(keys: TKey): void;
}
