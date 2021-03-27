import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { BacklogItemRelatedItem, BacklogRelationshipType } from '@core/api-models/common/backlog-item';
import { BacklogItemReference } from '@core/api-models/common/references';

@Component({
	selector: 'backlog-item-related-items',
	templateUrl: './related-items.component.html',
	styleUrls: ['./related-items.component.scss'],
})
export class BacklogItemRelatedItemsComponent implements OnInit {
	@Input()
	items: BacklogItemRelatedItem[] | undefined;
	@Output()
	itemsChanged: EventEmitter<{ [key: string]: keyof typeof BacklogRelationshipType }> = new EventEmitter();

	groupedItems: Record<keyof typeof BacklogRelationshipType, BacklogItemReference[]> | undefined;

	get types(): BacklogRelationshipType[] {
		return !!this.groupedItems ? (Object.keys(this.groupedItems) as Array<BacklogRelationshipType>) : [];
	}

	constructor() {}

	ngOnInit(): void {
		this.groupedItems = this.getGroupedItems(
			this.items,
			i => i.linkType,
			i => i.relatedTo!
		);
	}

	private getGroupedItems(
		list: BacklogItemRelatedItem[] | undefined,
		getKey: (item: BacklogItemRelatedItem) => keyof typeof BacklogRelationshipType,
		getResult: (item: BacklogItemRelatedItem) => BacklogItemReference
	): Record<keyof typeof BacklogRelationshipType, BacklogItemReference[]> {
		if (!list) return {} as Record<keyof typeof BacklogRelationshipType, BacklogItemReference[]>;

		return list.reduce((previous, currentItem) => {
			const group = getKey(currentItem);
			if (!previous[group]) previous[group] = [];
			previous[group].push(getResult(currentItem));
			return previous;
		}, {} as Record<keyof typeof BacklogRelationshipType, BacklogItemReference[]>);
	}
}
