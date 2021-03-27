import { BacklogItemReference } from '../references';
import { BacklogRelationshipType } from './BacklogRelationshipType';

export interface BacklogItemRelatedItem {
	relatedTo?: BacklogItemReference;
	linkType: keyof typeof BacklogRelationshipType;
}
