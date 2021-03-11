import { ChangedByUserReference } from '../references';

export interface BacklogItemHistoryRecord extends ChangedByUserReference {
	summary: string;
}
