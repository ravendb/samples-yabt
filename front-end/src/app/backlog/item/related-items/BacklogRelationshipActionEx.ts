import { BacklogRelationshipAction } from '@core/api-models/backlog-item/item/BacklogRelationshipAction';
import { BacklogItemType } from '@core/api-models/common/backlog-item';

export interface BacklogRelationshipActionEx extends BacklogRelationshipAction {
	backlogItemTitle: string;
	backlogItemType: keyof typeof BacklogItemType;
}
