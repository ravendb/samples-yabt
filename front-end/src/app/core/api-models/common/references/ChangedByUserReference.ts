import { UserReference } from './UserReference';

export class ChangedByUserReference {
	timestamp!: Date;
	actionedBy!: UserReference;
}
