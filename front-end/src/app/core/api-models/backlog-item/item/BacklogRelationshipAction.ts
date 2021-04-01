import { BacklogRelationshipType } from '@core/api-models/common/backlog-item';
import { ListActionType } from '@core/api-models/common/ListActionType';

export interface BacklogRelationshipAction {
	backlogItemId: string;
	relationType: keyof typeof BacklogRelationshipType;
	actionType: keyof typeof ListActionType;
}
