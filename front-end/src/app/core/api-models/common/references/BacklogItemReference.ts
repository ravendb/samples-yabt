import { BacklogItemType } from '../backlog-item';

export class BacklogItemReference {
	id?: string;
	name: string = '';
	type: BacklogItemType = null!;
}
