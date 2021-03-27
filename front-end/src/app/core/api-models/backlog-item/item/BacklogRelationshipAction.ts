import { BacklogRelationshipType } from '@core/api-models/common/backlog-item';
import { BacklogRelationshipActionType } from './BacklogRelationshipActionType';

export interface BacklogRelationshipAction {
	backlogItemId: string;
	relationType: keyof typeof BacklogRelationshipType;
	actionType: keyof typeof BacklogRelationshipActionType;
}
