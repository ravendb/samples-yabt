import { BacklogItemState, BacklogRelationshipType } from '@core/api-models/common/backlog-item';

export interface BacklogItemAddUpdRequestBase {
	title: string;
	state: BacklogItemState;
	estimatedSize: number | undefined;
	assigneeId: string | undefined;
	tags: string[];
	relatedItems: { [key: string]: BacklogRelationshipType };
	customFields: { [key: string]: any };
}
