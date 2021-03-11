import { BacklogItemType } from '../backlog-item';

export interface BacklogItemReference {
	id?: string;
	name: string;
	type: BacklogItemType;
}
