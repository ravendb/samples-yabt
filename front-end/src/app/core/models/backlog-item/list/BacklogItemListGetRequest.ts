import { BacklogItemType } from '@core/models/common/BacklogItemType';
import { ListRequest } from '@core/models/common/ListRequest';

export class BacklogItemListGetRequest extends ListRequest {
	type: BacklogItemType | undefined;
	tags: string[] | undefined;
	search: string | undefined;
	modifiedByTheCurrentUserOnly: boolean = false;
	mentionsOfTheCurrentUserOnly: boolean = false;
}
