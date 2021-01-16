import { AfterViewInit, Component, ContentChild, Input, OnDestroy, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatSelectionList } from '@angular/material/list';
import { compact, isArray, isEmpty, isString } from 'lodash-es';
import { debounceTime, delay, distinctUntilChanged, switchMap, tap } from 'rxjs/operators';
import { BaseFilterButtonComponent } from '../base-filter-button';
import { IKeyValuePair } from '../ikey-value-pair';

@Component({
	selector: 'filter-multi-select',
	styleUrls: ['./filter-multi-select.component.scss'],
	templateUrl: './filter-multi-select.component.html',
})
export class FilterMultiSelectComponent
	extends BaseFilterButtonComponent<string[], IKeyValuePair>
	implements OnInit, AfterViewInit, OnDestroy {
	// Set the API methods for searchable lists
	// For simplicity sake, here I favoured a single method vs an event+property bundle
	@Input('search')
	search: ((term: string) => Observable<IKeyValuePair[]>) | undefined;

	@ViewChild(MatSelectionList)
	list!: MatSelectionList;
	@ContentChild(TemplateRef)
	templateRef: TemplateRef<any> | undefined;

	loading = false;
	searchCtrl: FormControlTyped<string> = new FormControl('undefined');

	rectangleText: string = '';

	ngOnInit(): void {
		super.ngOnInit();

		if (!!this.search) {
			// Only for searchable lists
			this._subscriptions.add(
				this.searchCtrl.valueChanges
					.pipe(
						distinctUntilChanged(),
						debounceTime(300),
						tap(() => {
							this.loading = true;
							this.updateLabel([]);
							this.options = [];
						}),
						switchMap(term => this.search!(term)),
						tap(o => {
							this.loading = false;
							this.options = o;
						}),
						delay(0) // Need to update the list
					)
					.subscribe(() => this.updateLabel(this.control.value))
			);

			// Trigger request for tags
			this.searchCtrl.setValue('');
		}
	}
	ngAfterViewInit(): void {
		this._subscriptions.add(this.list.selectionChange.subscribe(() => this.applyFilter()));
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

	protected updateLabel(keys: string[]): void {
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
