import { AfterViewInit, Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { MatSelectionList } from '@angular/material/list';
import { compact, isArray, isEmpty, isEqual, isString } from 'lodash-es';
import { Subscription } from 'rxjs';
import { distinctUntilChanged } from 'rxjs/operators';
import { IKeyValuePair } from '../ikey-value-pair';

@Component({
	selector: 'filter-multi-select',
	styleUrls: ['./filter-multi-select.component.scss'],
	templateUrl: './filter-multi-select.component.html',
})
export class FilterMultiSelectComponent implements OnInit, AfterViewInit, OnDestroy {
	@ViewChild(MatSelectionList)
	list!: MatSelectionList;

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
	options: IKeyValuePair[] = [];
	@Input()
	control!: AbstractControl;

	buttonText: string = this.label;

	private _label: string = 'Unspecified';
	private _subscriptions: Subscription = new Subscription();

	ngOnInit() {
		this._subscriptions.add(this.control.valueChanges.subscribe((keys: string[]) => this.updateLabel(keys)));
	}
	ngAfterViewInit() {
		this._subscriptions.add(this.list.selectionChange.pipe(distinctUntilChanged(isEqual)).subscribe(() => this.applyFilter()));
	}
	ngOnDestroy() {
		this._subscriptions.unsubscribe();
	}
	applyFilter(): void {
		const keys: string[] = compact(
			this.list.selectedOptions.selected.map(option => this.options.find(item => option.value === item.key)?.key)
		);
		this.control.setValue(keys || undefined);
	}

	clear(): void {
		this.list.deselectAll();
		this.applyFilter();
	}

	private updateLabel(keys: string[]): void {
		if (isEmpty(keys)) this.buttonText = this.label;
		else {
			const firstKey = isArray(keys) ? keys[0] : keys;
			let newLabel = this.options.find(o => o.key === firstKey)?.value || '';
			if (isArray(keys) && keys.length > 1) newLabel += ' +1';
			this.buttonText = newLabel;
		}
		if (!isEmpty(this.options) && !isEmpty(this.list.options)) {
			const selectedKeys = this.getSetValues();
			this.list.options.forEach(option => option._setSelected(selectedKeys.findIndex(key => key === option.value) > -1));
		}
	}

	private getSetValues(): string[] {
		const selectedKeys = isArray(this.control.value)
			? this.control.value
			: isString(this.control.value)
			? [this.control.value as string]
			: [];
		return selectedKeys;
	}
}
