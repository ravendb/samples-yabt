import { ChangeDetectionStrategy, Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { delay } from 'rxjs/operators';
import { IKeyValuePair } from './ikey-value-pair';

@Component({
	selector: 'filter-dropdown',
	styleUrls: ['./filter-dropdown.component.scss'],
	templateUrl: './filter-dropdown.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FilterDropdownComponent implements OnInit, OnDestroy {
	@Input()
	label: string = 'Unspecified'; // Label needs to be provided
	@Input()
	buttonAltText: string = '';
	@Input()
	options: IKeyValuePair[] = [];
	@Input()
	firstUndefinedOption: string = '';
	@Input()
	control!: AbstractControl;

	buttonText: string = this.label;

	private subscriptions: Subscription = new Subscription();

	setType(value: string | null): void {
		const el = !!value ? this.options.find(v => v.key == value) : undefined;
		this.buttonText = el?.value || this.label;
		this.control.setValue(el?.key || undefined);
	}

	ngOnInit() {
		this.subscriptions.add(
			this.control.valueChanges
				.pipe(
					delay(0) // avoid getting an error on changing the state after it's been checked
				)
				.subscribe((key: string) => this.updateLabel(key))
		);
	}

	ngOnDestroy() {
		this.subscriptions.unsubscribe();
	}

	private updateLabel(key: string): void {
		const el = !!key ? this.options.find(v => v.key == key) : undefined;
		this.buttonText = el?.value || this.label;
	}
}
