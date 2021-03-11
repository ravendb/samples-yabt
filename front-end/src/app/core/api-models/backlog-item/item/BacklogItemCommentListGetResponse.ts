import { UserReference } from '@core/api-models/common/references/UserReference';

export interface BacklogItemCommentListGetResponse {
	id: string;
	message: string;
	author: UserReference;
	created: Date;
	lastUpdated: Date;
}
