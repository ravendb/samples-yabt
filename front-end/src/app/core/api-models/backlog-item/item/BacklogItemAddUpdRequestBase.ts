import { BacklogItemState } from '@core/api-models/common/backlog-item';
import { BacklogCustomFieldAction } from './BacklogCustomFieldAction';
import { BacklogRelationshipAction } from './BacklogRelationshipAction';

export interface BacklogItemAddUpdRequestBase {
	title: string;
	state: keyof typeof BacklogItemState;
	estimatedSize: number | undefined;
	assigneeId: string | undefined;
	tags: string[];
	changedRelatedItems: BacklogRelationshipAction[] | undefined;
	changedCustomFields: BacklogCustomFieldAction[] | undefined;
}
