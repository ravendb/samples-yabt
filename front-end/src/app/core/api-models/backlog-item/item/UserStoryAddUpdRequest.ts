import { BacklogItemAddUpdRequestBase } from './BacklogItemAddUpdRequestBase';

export class UserStoryAddUpdRequest extends BacklogItemAddUpdRequestBase {
	acceptanceCriteria: string | undefined;
}
