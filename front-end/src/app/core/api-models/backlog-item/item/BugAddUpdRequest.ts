import { BugPriority, BugSeverity } from '@core/api-models/common/backlog-item';
import { BacklogItemAddUpdRequestBase } from './BacklogItemAddUpdRequestBase';

export interface BugAddUpdRequest extends BacklogItemAddUpdRequestBase {
	severity: keyof typeof BugSeverity | undefined;
	priority: keyof typeof BugPriority | undefined;
	stepsToReproduce: string | undefined;
	acceptanceCriteria: string | undefined;
}
