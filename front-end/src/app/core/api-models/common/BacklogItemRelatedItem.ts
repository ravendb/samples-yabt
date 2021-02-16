import { BacklogRelationshipType } from './BacklogRelationshipType';
import { BacklogItemReference } from './references';

export class BacklogItemRelatedItem {
	relatedTo?: BacklogItemReference;
	linkType: BacklogRelationshipType = null!;

	constructor() {}
}
