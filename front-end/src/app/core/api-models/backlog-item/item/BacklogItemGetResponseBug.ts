import { BugPriority, BugSeverity } from '@core/api-models/common/backlog-item';
import { BacklogItemGetResponseBase } from './BacklogItemGetResponseBase';

export interface BacklogItemGetResponseBug extends BacklogItemGetResponseBase {
	severity: BugSeverity;
	priority: BugPriority;
	stepsToReproduce: string;
}
