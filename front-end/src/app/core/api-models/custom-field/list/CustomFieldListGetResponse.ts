import { CustomFieldType } from '@core/api-models/common/CustomFieldType';

export class CustomFieldListGetResponse {
	id!: string;
	name!: string;
	fieldType!: CustomFieldType;
}
