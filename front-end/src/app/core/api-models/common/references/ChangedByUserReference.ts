import { UserReference } from './UserReference';

export interface ChangedByUserReference {
	timestamp: Date;
	actionedBy: UserReference;
}
