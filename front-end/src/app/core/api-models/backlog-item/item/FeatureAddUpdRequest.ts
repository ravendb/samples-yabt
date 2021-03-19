import { BacklogItemAddUpdRequestBase } from './BacklogItemAddUpdRequestBase';

export interface FeatureAddUpdRequest extends BacklogItemAddUpdRequestBase {
	description: string;
}
