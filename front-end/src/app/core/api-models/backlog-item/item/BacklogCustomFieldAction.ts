import { ListActionType } from '@core/api-models/common/ListActionType';

export interface BacklogCustomFieldAction {
	customFieldId: string;
	value: any;
	actionType: keyof typeof ListActionType;
}
