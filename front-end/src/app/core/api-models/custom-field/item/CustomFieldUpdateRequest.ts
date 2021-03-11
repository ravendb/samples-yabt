import { BacklogItemType } from '@core/api-models/common/backlog-item';

export class CustomFieldUpdateRequest {
	name!: string;
	isMandatory!: boolean;
	backlogItemTypes: Array<BacklogItemType> | undefined;
}
