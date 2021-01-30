import { BacklogItemType } from '@core/api-models/common/BacklogItemType';

export class CustomFieldUpdateRequest {
	name!: string;
	isMandatory!: boolean;
	backlogItemTypes: Array<BacklogItemType> | undefined;
}
