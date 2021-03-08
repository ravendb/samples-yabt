import { BugAddUpdRequest } from './BugAddUpdRequest';
import { UserStoryAddUpdRequest } from './UserStoryAddUpdRequest';

export type BacklogAddUpdAllFieldsRequest = BugAddUpdRequest & UserStoryAddUpdRequest;
