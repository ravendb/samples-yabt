import { BugPriority } from '@core/api-models/common/BugPriority';
import { BugSeverity } from '@core/api-models/common/BugSeverity';
import { BacklogItemAddUpdRequestBase } from './BacklogItemAddUpdRequestBase';

export class BugAddUpdRequest extends BacklogItemAddUpdRequestBase {
	severity: BugSeverity | undefined;
	priority: BugPriority | undefined;
	stepsToReproduce: string | undefined;
	acceptanceCriteria: string | undefined;
}
