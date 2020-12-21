import { BacklogItemType } from '@core/models/common/BacklogItemType';
import { ListRequest } from '@core/models/common/ListRequest';

export class BacklogItemListGetRequest extends ListRequest {
	type: keyof typeof BacklogItemType | undefined;
	tags: string[] | undefined;
	search: string | undefined;
	assignedUserId: string | undefined;
	modifiedByTheCurrentUserOnly: boolean = false;
	mentionsOfTheCurrentUserOnly: boolean = false;
}
