import { AfterViewInit, Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatSelectionList } from '@angular/material/list';
import { BacklogItemTagListGetRequest } from '@core/models/backlog-item/list';
import { BacklogItemsService } from '@core/services/backlogItems.service';
import { compact, isArray, isEmpty, isString } from 'lodash-es';
import { Subscription } from 'rxjs';
import { debounceTime, delay, distinctUntilChanged, map, switchMap, tap } from 'rxjs/operators';

@Component({
	selector: 'filter-tag-select',
	styleUrls: ['./filter-tag-select.component.scss'],
	templateUrl: './filter-tag-select.component.html',
})
export class FilterTagSelectComponent implements OnInit, AfterViewInit, OnDestroy {
	@ViewChild(MatSelectionList)
	list!: MatSelectionList;

	@Input()
	control!: AbstractControl;

	private _label: string = 'Tags';
	private _subscriptions: Subscription = new Subscription();

	loading = false;
	options: string[] | undefined;
	buttonText: string = this._label;
	searchCtrl: FormControlTyped<string> = new FormControl('undefined');

	constructor(private apiService: BacklogItemsService) {}

	ngOnInit() {
		this._subscriptions.add(
			this.searchCtrl.valueChanges
				.pipe(
					distinctUntilChanged(),
					debounceTime(300),
					tap(() => {
						this.loading = true;
						this.options = [];
					}),
					switchMap(term => this.search(term)),
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

		// Update the label on changing the selection
		this._subscriptions.add(this.control.valueChanges.subscribe((keys: string[]) => this.updateLabel(keys)));
	}
	ngAfterViewInit() {
		this._subscriptions.add(this.list.selectionChange.subscribe(() => this.applyFilter()));
	}
	ngOnDestroy() {
		this._subscriptions.unsubscribe();
	}

	applyFilter(): void {
		const keys: string[] = compact(this.list.selectedOptions.selected.map(option => option.value));

		this.control.setValue(keys || undefined);
	}

	clear(): void {
		this.list.deselectAll();
		this.applyFilter();
	}

	private search(search: string): Observable<string[]> {
		return this.apiService.getBacklogItemTagList({ search } as BacklogItemTagListGetRequest).pipe(map(tags => tags.map(t => t.name)));
	}

	private updateLabel(keys: string[]): void {
		if (isEmpty(keys)) this.buttonText = this._label;
		else {
			const firstKey = isArray(keys) ? keys[0] : keys;
			let newLabel = this.options?.find(o => o === firstKey) || '';
			if (isArray(keys) && keys.length > 1) newLabel += ' +1';
			this.buttonText = newLabel;
		}
		if (!isEmpty(this.options) && !isEmpty(this.list.options)) {
			const selectedKeys = this.getSetValues();
			console.debug('!!!2 ' + this.list.options.length);
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
