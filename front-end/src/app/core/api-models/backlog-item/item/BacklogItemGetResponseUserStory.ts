import { BacklogItemGetResponseBase } from './BacklogItemGetResponseBase';

export interface BacklogItemGetResponseUserStory extends BacklogItemGetResponseBase {
	acceptanceCriteria: string;
}
