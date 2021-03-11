import { BugPriority, BugSeverity } from '@core/api-models/common/backlog-item';
import { BacklogItemAddUpdRequestBase } from './BacklogItemAddUpdRequestBase';

export interface BugAddUpdRequest extends BacklogItemAddUpdRequestBase {
	severity: BugSeverity | undefined;
	priority: BugPriority | undefined;
	stepsToReproduce: string | undefined;
	acceptanceCriteria: string | undefined;
}
