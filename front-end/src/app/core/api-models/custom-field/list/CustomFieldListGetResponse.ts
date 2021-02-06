import { BacklogItemType } from '@core/api-models/common/BacklogItemType';
import { CustomFieldType } from '@core/api-models/common/CustomFieldType';

export class CustomFieldListGetResponse {
	id!: string;
	name!: string;
	fieldType!: CustomFieldType;
	isMandatory!: boolean;
	backlogItemTypes: Array<BacklogItemType> | undefined;
}