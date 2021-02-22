import { BacklogItemRelatedItem } from '@core/api-models/common/BacklogItemRelatedItem';
import { BacklogItemState } from '@core/api-models/common/BacklogItemState';
import { BacklogItemType } from '@core/api-models/common/BacklogItemType';
import { ChangedByUserReference } from '@core/api-models/common/references/ChangedByUserReference';
import { UserReference } from '@core/api-models/common/references/UserReference';
import { BacklogItemCommentListGetResponse } from './BacklogItemCommentListGetResponse';

export class BacklogItemGetResponseBase {
	title!: string;
	state!: BacklogItemState;
	estimatedSize: number | undefined;
	type!: keyof typeof BacklogItemType;
	assignee: UserReference | undefined;
	created!: ChangedByUserReference;
	lastUpdated!: ChangedByUserReference;
	tags: string[] = [];
	comments: BacklogItemCommentListGetResponse[] = [];
	customFields: { [key: string]: any } = {};
	relatedItems: BacklogItemRelatedItem[] = [];
}
