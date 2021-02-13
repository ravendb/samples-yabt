import { BacklogRelationshipType } from '@core/api-models/common/BacklogRelationshipType';
import { UserReference } from '@core/api-models/common/references/UserReference';

export class BacklogItemAddUpdRequestBase {
	title!: string;
	assigneeId: UserReference | undefined;
	tags: string[] = [];
	relatedItems: { [key: string]: BacklogRelationshipType } = {};
	customFields: { [key: string]: any } = {};
}
