import { BacklogItemCommentListGetResponse } from '@core/api-models/backlog-item/item/BacklogItemCommentListGetResponse';
import { BacklogItemType } from '@core/api-models/common/BacklogItemType';
import { ChangedByUserReference } from '@core/api-models/common/references';

export interface BacklogItemReadonlyProperties {
	type: keyof typeof BacklogItemType;
	created: ChangedByUserReference;
	lastUpdated: ChangedByUserReference;
	comments: BacklogItemCommentListGetResponse[];
}