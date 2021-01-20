import { Directive, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ListRequest } from '@core/api-models/common/ListRequest';
import { generateFormGroupFromObject } from '@utils/abstract-control';
import { isArray, isEmpty, isNil, map } from 'lodash-es';
import { merge, Subscription } from 'rxjs';
import { delay, distinctUntilChanged } from 'rxjs/operators';

@Directive()
export class FilterBarComponentBase<TFilter extends ListRequest> implements OnInit, OnDestroy {
	protected subscription = new Subscription();
	protected _filter: Partial<TFilter> = {};

	// The filter must be initialised. Otherwise the whole filtering is going south
	@Input()
	set filter(value: Partial<TFilter>) {
		this._filter = value;
		this.resetFilter(value);
	}
	@Output() filterChange = new EventEmitter<Partial<TFilter>>();

	formGroup!: FormGroupTyped<TFilter>;

	constructor(private fb: FormBuilder) {}

	ngOnInit(): void {
		this.formGroup = generateFormGroupFromObject(this.fb, this._filter);

		const triggers = map(this.formGroup.controls, (c: AbstractControl) => c.valueChanges.pipe(distinctUntilChanged()));

		this.subscription.add(
			merge(...triggers)
				.pipe(delay(0))
				.subscribe(() => this.applyFilter())
		);
	}

	ngOnDestroy(): void {
		this.subscription.unsubscribe();
	}

	clearFilters(): void {
		this.resetFilter({} as TFilter);
	}

	protected setFilter(newValues: Partial<TFilter>) {
		if (!!this.formGroup) this.formGroup.setValue({ ...this._filter, ...newValues } as TFilter);
	}

	protected resetFilter(newValues: Partial<TFilter>) {
		if (!!this.formGroup) this.formGroup.reset(newValues as TFilter);
	}

	protected applyFilter(): void {
		this.filterChange.emit(this.formGroup.value);
	}

	protected isNull(item: any): boolean {
		// ignore empty values as well as empty arrays
		return isNil(item) || (isArray(item) && isEmpty(item));
	}
}
