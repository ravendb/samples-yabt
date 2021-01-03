import { BacklogItemType } from '@core/models/common/BacklogItemType';
import { ChangedByUserReference } from '@core/models/common/references/ChangedByUserReference';
import { UserReference } from '@core/models/common/references/UserReference';

export class BacklogItemListGetResponse {
	id!: string;
	title!: string;
	assignee: UserReference | undefined;
	type: BacklogItemType | undefined;
	created!: ChangedByUserReference;
	lastUpdated!: ChangedByUserReference;
}
