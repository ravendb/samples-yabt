import { ListRequest } from '@core/api-models/common/ListRequest';

export class CustomFieldListGetRequest extends ListRequest {
	ids: string[] | undefined;
}
