import { WorkflowStepType } from '../workflows/workflow.model';

export type WorkflowInstanceStatus = 'InProgress' | 'Approved' | 'Rejected' | 'Completed';
export type WorkflowStepInstanceStatus = 'Pending' | 'InProgress' | 'Completed' | 'Skipped';

export interface WorkflowInstance {
  id: number;
  workflowDefinitionId: number;
  workflowDefinitionName: string;
  applicationId: number;
  applicationName: string;
  formSubmissionId: number;
  submittedByUserName: string;
  currentStepId: number | null;
  currentStepName: string | null;
  status: WorkflowInstanceStatus;
  startedDate: string;
  completedDate: string | null;
}

export interface WorkflowStepInstance {
  id: number;
  stepName: string;
  stepType: WorkflowStepType;
  status: WorkflowStepInstanceStatus;
  outcome: string | null;
  enteredDate: string;
  exitedDate: string | null;
}
