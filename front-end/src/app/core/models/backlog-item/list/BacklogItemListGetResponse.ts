import { BacklogItemType } from '@core/models/common/BacklogItemType';
import { ChangedByUserReference } from '@core/models/common/references/ChangedByUserReference';
import { UserReference } from '@core/models/common/references/UserReference';

export class BacklogItemListGetResponse {
	id!: string;
	title!: string;
	assignee: UserReference | null = null;
	type: BacklogItemType = BacklogItemType.Unknown;
	created!: ChangedByUserReference;
	lastUpdated!: ChangedByUserReference;
}
