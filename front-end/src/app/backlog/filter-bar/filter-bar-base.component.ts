import { Directive, EventEmitter, Input, Output } from '@angular/core';
import { ListRequest } from '@core/models/common/ListRequest';

@Directive()
export class FilterBarComponentBase<TFilter extends ListRequest> {
	protected _filter: Partial<TFilter> = {};

	@Input()
	set filter(value: Partial<TFilter>) {
		this._filter = value;
	}
	@Output() filterChange = new EventEmitter<Partial<TFilter>>();
}
