import { BacklogItemType } from '../BacklogItemType';

export class BacklogItemReference {
	id?: string;
	name: string = '';
	type: BacklogItemType = null!;
}
