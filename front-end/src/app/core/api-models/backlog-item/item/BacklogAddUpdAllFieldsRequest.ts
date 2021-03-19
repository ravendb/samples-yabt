import { BugAddUpdRequest } from './BugAddUpdRequest';
import { FeatureAddUpdRequest } from './FeatureAddUpdRequest';
import { TaskAddUpdRequest } from './TaskAddUpdRequest';
import { UserStoryAddUpdRequest } from './UserStoryAddUpdRequest';

export type BacklogAddUpdAllFieldsRequest = BugAddUpdRequest & UserStoryAddUpdRequest & TaskAddUpdRequest & FeatureAddUpdRequest;
