import { Directive, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ListRequest } from '@core/models/common/ListRequest';
import { isArray, isEmpty, isNil } from 'lodash-es';
import { Subscription } from 'rxjs';

@Directive()
export class FilterBarComponentBase<TFilter extends ListRequest> implements OnInit {
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
		this.formGroup = this.fb.group(
			Object.keys(this._filter).reduce(
				(prev, curr) => Object.assign(prev, { [curr]: this._filter[curr as keyof TFilter] instanceof Array ? [] : null }),
				{}
			)
		) as FormGroupTyped<TFilter>;
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
