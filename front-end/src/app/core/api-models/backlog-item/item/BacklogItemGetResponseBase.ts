import { BacklogItemHistoryRecord, BacklogItemRelatedItem, BacklogItemState, BacklogItemType } from '@core/api-models/common/backlog-item';
import { ChangedByUserReference } from '@core/api-models/common/references/ChangedByUserReference';
import { UserReference } from '@core/api-models/common/references/UserReference';
import { BacklogItemCommentListGetResponse } from './BacklogItemCommentListGetResponse';

export interface BacklogItemGetResponseBase {
	title: string;
	state: BacklogItemState;
	estimatedSize: number | undefined;
	assignee: UserReference | undefined;
	type: keyof typeof BacklogItemType;
	historyDescOrder: BacklogItemHistoryRecord[];
	created: ChangedByUserReference;
	lastUpdated: ChangedByUserReference;
	tags: string[];
	comments: BacklogItemCommentListGetResponse[];
	customFields: { [key: string]: any };
	relatedItems: BacklogItemRelatedItem[];
}
