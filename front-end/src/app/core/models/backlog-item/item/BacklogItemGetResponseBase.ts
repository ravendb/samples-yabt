import { BacklogItemType } from '@core/models/common/BacklogItemType';
import { ChangedByUserReference } from '@core/models/common/references/ChangedByUserReference';
import { UserReference } from '@core/models/common/references/UserReference';
import { BacklogItemCommentListGetResponse } from './BacklogItemCommentListGetResponse';

export class BacklogItemGetResponseBase {
	title!: string;
	type: BacklogItemType = BacklogItemType.Unknown;
	assignee: UserReference | null = null;
	created!: ChangedByUserReference;
	lastUpdated!: ChangedByUserReference;
	tags: string[] = [];
	comments: BacklogItemCommentListGetResponse[] = [];
	customFields: { [key: string]: any } = {};
}
