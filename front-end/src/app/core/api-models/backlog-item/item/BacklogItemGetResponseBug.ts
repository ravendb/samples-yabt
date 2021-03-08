import { BugPriority } from '@core/api-models/common/BugPriority';
import { BugSeverity } from '@core/api-models/common/BugSeverity';
import { BacklogItemGetResponseBase } from './BacklogItemGetResponseBase';

export class BacklogItemGetResponseBug extends BacklogItemGetResponseBase {
	severity!: BugSeverity;
	priority!: BugPriority;
	stepsToReproduce!: string;
}
