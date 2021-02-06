import { Directive, Input, OnDestroy, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Subscription } from 'rxjs';
import { debounceTime, delay, distinctUntilChanged, switchMap, take, tap } from 'rxjs/operators';
import { IKeyValuePair } from '../ikey-value-pair';
import { BaseFilterButtonComponent } from './base-filter-button.component';

@Directive()
export abstract class BaseSearchableFilterButtonComponent<TKey> extends BaseFilterButtonComponent<TKey> implements OnInit, OnDestroy {
	// Set the API methods for searchable lists
	// For simplicity sake, here I favoured a single method vs an event+property bundle
	@Input('search')
	search: ((term: string) => Observable<IKeyValuePair[]>) | undefined;

	loading = false;
	searchCtrl: FormControlTyped<string> = new FormControl('');

	protected _subscriptions: Subscription = new Subscription();

	ngOnInit(): void {
		if (!!this.search) {
			// Only for searchable lists
			this._subscriptions.add(
				this.searchCtrl.valueChanges
					.pipe(
						distinctUntilChanged(),
						debounceTime(300),
						tap(() => {
							this.loading = true;
							this.options = [];
						}),
						switchMap(term => this.search!(term)),
						tap(o => {
							this.loading = false;
							this.options = o;
						}),
						delay(0) // Need to update the list
					)
					.subscribe()
			);

			// Trigger request to fulfil the list (could use setting value in the search, but the handler above has a 300ms delay)
			this.search!('')
				.pipe(
					take(1),
					tap(o => (this.options = o)),
					delay(0)
				)
				.subscribe(() => {
					this.initiateList();
					this.updateLabel(this.value);
				});
		}
	}

	ngOnDestroy() {
		this._subscriptions.unsubscribe();
	}

	protected abstract initiateList(): void;
}
