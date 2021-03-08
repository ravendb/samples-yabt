import { ListRequest } from '@core/api-models/common/ListRequest';

export class UserListGetRequest extends ListRequest {
	search: string | undefined;
}
