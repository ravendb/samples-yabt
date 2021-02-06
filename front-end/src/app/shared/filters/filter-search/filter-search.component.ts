import { Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ControlContainer, ControlValueAccessor, FormControl, FormControlDirective, NG_VALUE_ACCESSOR } from '@angular/forms';
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
export class FilterSearchComponent implements ControlValueAccessor, OnInit, OnDestroy {
	@ViewChild(FormControlDirective, { static: true })
	formControlDirective!: FormControlDirective;
	@Input()
	formControl: FormControl | undefined;
	@Input()
	formControlName: string | undefined;

	/* 
		Resolve FormControl instance no matter `formControl` or `formControlName` is given. 
		If formControlName is given, then this.controlContainer.control is the parent FormGroup (or FormArray) instance. 
	*/
	get control(): AbstractControl | undefined {
		return this.formControl || (!!this.formControlName && this.controlContainer.control?.get(this.formControlName)) || undefined;
	}

	searchControl: FormControlTyped<string> = new FormControl('');

	/* View -> model callback called when select has been touched */
	public onTouched: () => void = () => {};
	/* View -> model callback called when value changes from the UI */
	private _onChange: (value?: string) => void = () => {};

	private _subscriptions: Subscription = new Subscription();

	constructor(private controlContainer: ControlContainer) {}

	ngOnInit() {
		this._subscriptions.add(
			this.searchControl.valueChanges.pipe(distinctUntilChanged(), debounceTime(300)).subscribe((val: string) => this._onChange(val))
		);
	}
	ngOnDestroy() {
		this._subscriptions.unsubscribe();
	}

	registerOnChange(fn: (value?: string) => void): void {
		this._onChange = fn;
	}
	registerOnTouched(fn: () => {}): void {
		this.onTouched = fn;
	}
	writeValue(val: string): void {
		this.searchControl.setValue(val, { emitEvent: false });
	}
	setDisabledState(isDisabled: boolean): void {
		if (isDisabled) this.searchControl.disable();
		else this.searchControl.enable();
	}
}
