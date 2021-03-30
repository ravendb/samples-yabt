import { BacklogItemType } from '@core/api-models/common/backlog-item';
import { ChangedByUserReference } from '@core/api-models/common/references/ChangedByUserReference';
import { UserReference } from '@core/api-models/common/references/UserReference';

export interface BacklogItemListGetResponse {
	id: string;
	title: string;
	assignee: UserReference | undefined;
	commentsCount: number;
	type: keyof typeof BacklogItemType;
	created: ChangedByUserReference;
	lastUpdated: ChangedByUserReference;
}
