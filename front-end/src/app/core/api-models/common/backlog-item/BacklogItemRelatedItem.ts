import { BacklogItemReference } from '../references';
import { BacklogRelationshipType } from './BacklogRelationshipType';

export class BacklogItemRelatedItem {
	relatedTo?: BacklogItemReference;
	linkType: BacklogRelationshipType = null!;

	constructor() {}
}
