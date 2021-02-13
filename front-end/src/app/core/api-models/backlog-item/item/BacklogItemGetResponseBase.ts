import { BacklogItemType } from '@core/api-models/common/BacklogItemType';
import { ChangedByUserReference } from '@core/api-models/common/references/ChangedByUserReference';
import { UserReference } from '@core/api-models/common/references/UserReference';
import { BacklogItemCommentListGetResponse } from './BacklogItemCommentListGetResponse';

export class BacklogItemGetResponseBase {
	title!: string;
	type!: keyof typeof BacklogItemType;
	assignee: UserReference | undefined;
	created!: ChangedByUserReference;
	lastUpdated!: ChangedByUserReference;
	tags: string[] = [];
	comments: BacklogItemCommentListGetResponse[] = [];
	customFields: { [key: string]: any } = {};
}
