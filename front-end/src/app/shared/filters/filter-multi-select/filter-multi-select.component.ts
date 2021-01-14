import { AfterViewInit, Component, ContentChild, Input, OnDestroy, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { MatSelectionList } from '@angular/material/list';
import { compact, isArray, isEmpty, isString } from 'lodash-es';
import { Subscription } from 'rxjs';
import { delay } from 'rxjs/operators';
import { IKeyValuePair } from '../ikey-value-pair';

@Component({
	selector: 'filter-multi-select',
	styleUrls: ['./filter-multi-select.component.scss'],
	templateUrl: './filter-multi-select.component.html',
})
export class FilterMultiSelectComponent implements OnInit, AfterViewInit, OnDestroy {
	@ViewChild(MatSelectionList)
	list!: MatSelectionList;
	@ContentChild(TemplateRef)
	templateRef: TemplateRef<any> | undefined;

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
	@Input()
	stretchedAndStroked: boolean = false;

	buttonText: string = this.label;
	rectangleText: string = '';

	private _label: string = 'Unspecified';
	private _subscriptions: Subscription = new Subscription();

	ngOnInit() {
		this._subscriptions.add(this.control.valueChanges.pipe(delay(0)).subscribe((keys: string[]) => this.updateLabel(keys)));
	}
	ngAfterViewInit() {
		this._subscriptions.add(this.list.selectionChange.subscribe(() => this.applyFilter()));
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
			if (!isArray(keys)) keys = [keys];
			const values = this.options.filter(o => keys.includes(o.key)).map(o => o.value);

			let newButtonText = values.find(Boolean) || '';
			if (values.length > 1) newButtonText += ' +1';
			this.buttonText = newButtonText;

			this.rectangleText = values.join(', ');
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
