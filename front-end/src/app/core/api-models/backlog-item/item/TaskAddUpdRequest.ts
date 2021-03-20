import { BacklogItemAddUpdRequestBase } from './BacklogItemAddUpdRequestBase';

export interface TaskAddUpdRequest extends BacklogItemAddUpdRequestBase {
	description: string;
}
