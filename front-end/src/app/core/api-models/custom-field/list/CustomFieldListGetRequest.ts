import { BacklogItemType } from '@core/api-models/common/backlog-item';
import { ListRequest } from '@core/api-models/common/ListRequest';

export class CustomFieldListGetRequest extends ListRequest {
	ids: string[] | undefined;
	backlogItemType: keyof typeof BacklogItemType | undefined;
}
