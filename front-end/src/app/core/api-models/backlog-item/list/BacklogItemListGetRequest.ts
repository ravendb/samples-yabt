import { BacklogItemState, BacklogItemType } from '@core/api-models/common/backlog-item';
import { ListRequest } from '@core/api-models/common/ListRequest';
import { CurrentUserRelations } from './CurrentUserRelations';

export class BacklogItemListGetRequest extends ListRequest {
	types: Array<keyof typeof BacklogItemType> | undefined;
	states: Array<keyof typeof BacklogItemState> | undefined;
	tags: string[] | undefined;
	search: string | undefined;
	assignedUserId: string | undefined;
	currentUserRelation: keyof typeof CurrentUserRelations | undefined;
}
