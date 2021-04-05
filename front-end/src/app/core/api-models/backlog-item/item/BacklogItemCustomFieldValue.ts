import { CustomFieldType } from '@core/api-models/common/CustomFieldType';

export interface BacklogItemCustomFieldValue {
	customFieldId: string;
	name: string;
	type: keyof typeof CustomFieldType;
	isMandatory: boolean;
	value: any;
}
