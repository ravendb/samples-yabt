import { BacklogItemHistoryRecord, BacklogItemRelatedItem, BacklogItemState, BacklogItemType } from '@core/api-models/common/backlog-item';
import { ChangedByUserReference } from '@core/api-models/common/references/ChangedByUserReference';
import { UserReference } from '@core/api-models/common/references/UserReference';
import { BacklogItemCommentListGetResponse } from './BacklogItemCommentListGetResponse';
import { BacklogItemCustomFieldValue } from './BacklogItemCustomFieldValue';

export interface BacklogItemGetResponseBase {
	title: string;
	state: keyof typeof BacklogItemState;
	estimatedSize: number | undefined;
	assignee: UserReference | undefined;
	type: keyof typeof BacklogItemType;
	historyDescOrder: BacklogItemHistoryRecord[];
	created: ChangedByUserReference;
	lastUpdated: ChangedByUserReference;
	tags: string[];
	comments: BacklogItemCommentListGetResponse[];
	customFields: BacklogItemCustomFieldValue[] | undefined;
	relatedItems: BacklogItemRelatedItem[] | undefined;
}
