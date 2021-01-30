import { BacklogItemType } from '@core/api-models/common/BacklogItemType';
import { CustomFieldType } from '@core/api-models/common/CustomFieldType';

export class CustomFieldAddRequest {
	name!: string;
	type!: CustomFieldType;
	isMandatory: boolean = false;
	backlogItemTypes: Array<BacklogItemType> | undefined;
}
