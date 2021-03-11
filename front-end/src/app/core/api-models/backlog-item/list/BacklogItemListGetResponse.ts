import { BacklogItemType } from '@core/api-models/common/backlog-item';
import { ChangedByUserReference } from '@core/api-models/common/references/ChangedByUserReference';
import { UserReference } from '@core/api-models/common/references/UserReference';

export class BacklogItemListGetResponse {
	id!: string;
	title!: string;
	assignee: UserReference | undefined;
	commentsCount!: number;
	type: BacklogItemType | undefined;
	created!: ChangedByUserReference;
	lastUpdated!: ChangedByUserReference;
}
