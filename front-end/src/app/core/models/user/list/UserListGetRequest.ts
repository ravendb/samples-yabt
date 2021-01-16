import { ListRequest } from '@core/models/common/ListRequest';

export class UserListGetRequest extends ListRequest {
	search: string | undefined;
}
