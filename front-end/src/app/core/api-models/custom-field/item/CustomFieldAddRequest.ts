import { BacklogItemType } from '@core/api-models/common/backlog-item';
import { CustomFieldType } from '@core/api-models/common/CustomFieldType';

export class CustomFieldAddRequest {
	name!: string;
	fieldType!: CustomFieldType;
	isMandatory: boolean = false;
	backlogItemTypes: Array<BacklogItemType> | undefined;
}
