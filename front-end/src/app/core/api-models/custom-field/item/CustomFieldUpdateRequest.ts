import { BacklogItemType } from '@core/api-models/common/backlog-item';

export interface CustomFieldUpdateRequest {
	name: string;
	isMandatory: boolean;
	backlogItemTypes: Array<BacklogItemType> | undefined;
}
