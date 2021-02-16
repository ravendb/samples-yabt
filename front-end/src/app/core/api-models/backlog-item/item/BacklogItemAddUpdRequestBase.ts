import { BacklogItemState } from '@core/api-models/common/BacklogItemState';
import { BacklogRelationshipType } from '@core/api-models/common/BacklogRelationshipType';

export class BacklogItemAddUpdRequestBase {
	title!: string;
	state!: BacklogItemState;
	assigneeId: string | undefined;
	tags: string[] = [];
	relatedItems: { [key: string]: BacklogRelationshipType } = {};
	customFields: { [key: string]: any } = {};
}
