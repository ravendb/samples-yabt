import { ListActionType } from '@core/api-models/common/ListActionType';

export interface BacklogCustomFieldAction {
	customFieldId: string;
	value: any | undefined;
	actionType: keyof typeof ListActionType;
}
