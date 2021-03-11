import { BacklogItemAddUpdRequestBase } from './BacklogItemAddUpdRequestBase';

export interface UserStoryAddUpdRequest extends BacklogItemAddUpdRequestBase {
	acceptanceCriteria: string | undefined;
}
