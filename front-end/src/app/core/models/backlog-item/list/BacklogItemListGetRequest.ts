import { BacklogItemState } from '@core/models/common/BacklogItemState';
import { BacklogItemType } from '@core/models/common/BacklogItemType';
import { ListRequest } from '@core/models/common/ListRequest';
import { CurrentUserRelations } from './CurrentUserRelations';

export class BacklogItemListGetRequest extends ListRequest {
	type: keyof typeof BacklogItemType | undefined;
	states: Array<keyof typeof BacklogItemState> | undefined;
	tags: string[] | undefined;
	search: string | undefined;
	assignedUserId: string | undefined;
	currentUserRelation: keyof typeof CurrentUserRelations | undefined;
}
