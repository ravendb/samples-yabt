import { BacklogItemType } from '@core/api-models/common/backlog-item';
import { CustomFieldType } from '@core/api-models/common/CustomFieldType';

export interface CustomFieldListGetResponse {
	id: string;
	name: string;
	fieldType: CustomFieldType;
	isMandatory: boolean;
	backlogItemTypes: Array<BacklogItemType> | undefined;
}
