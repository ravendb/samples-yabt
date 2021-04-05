import { BacklogItemCommentListGetResponse } from '@core/api-models/backlog-item/item/BacklogItemCommentListGetResponse';
import { BacklogItemCustomFieldValue } from '@core/api-models/backlog-item/item/BacklogItemCustomFieldValue';
import { BacklogItemHistoryRecord, BacklogItemRelatedItem, BacklogItemType } from '@core/api-models/common/backlog-item';
import { ChangedByUserReference } from '@core/api-models/common/references';

export interface BacklogItemReadonlyProperties {
	type: keyof typeof BacklogItemType;
	created: ChangedByUserReference;
	lastUpdated: ChangedByUserReference;
	comments: BacklogItemCommentListGetResponse[];
	historyDescOrder: BacklogItemHistoryRecord[];
	relatedItems: BacklogItemRelatedItem[] | undefined;
	customFields: BacklogItemCustomFieldValue[] | undefined;
}
