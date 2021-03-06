import { BacklogItemCommentListGetResponse } from '@core/api-models/backlog-item/item/BacklogItemCommentListGetResponse';
import { BacklogItemHistoryRecord, BacklogItemType } from '@core/api-models/common/backlog-item';
import { ChangedByUserReference } from '@core/api-models/common/references';

export interface BacklogItemReadonlyProperties {
	type: keyof typeof BacklogItemType;
	created: ChangedByUserReference;
	lastUpdated: ChangedByUserReference;
	comments: BacklogItemCommentListGetResponse[];
	historyDescOrder: BacklogItemHistoryRecord[];
}
