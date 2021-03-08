import { BacklogItemGetResponseBase } from './BacklogItemGetResponseBase';

export class BacklogItemGetResponseUserStory extends BacklogItemGetResponseBase {
	acceptanceCriteria!: string;
}
