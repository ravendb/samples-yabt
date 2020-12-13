import { UserReference } from '@core/models/common/references/UserReference';

export class BacklogItemCommentListGetResponse {
	id!: string;
	message!: string;
	author!: UserReference;
	created!: Date;
	lastUpdated!: Date;
}
