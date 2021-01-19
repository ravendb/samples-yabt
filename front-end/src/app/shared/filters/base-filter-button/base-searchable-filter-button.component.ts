import { Directive, Input, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { debounceTime, delay, distinctUntilChanged, switchMap, tap } from 'rxjs/operators';
import { IKeyValuePair } from '../ikey-value-pair';
import { BaseFilterButtonComponent } from './base-filter-button.component';

@Directive()
export abstract class BaseSearchableFilterButtonComponent<TKey> extends BaseFilterButtonComponent<TKey> implements OnInit {
	// Set the API methods for searchable lists
	// For simplicity sake, here I favoured a single method vs an event+property bundle
	@Input('search')
	search: ((term: string) => Observable<IKeyValuePair[]>) | undefined;

	loading = false;
	searchCtrl: FormControlTyped<string> = new FormControl('undefined');

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
							this.updateLabel({} as TKey);
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
}
