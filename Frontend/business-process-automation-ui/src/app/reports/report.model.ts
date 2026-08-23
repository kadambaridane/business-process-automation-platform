import { WorkflowInstanceStatus } from '../workflow-instances/workflow-instance.model';

export interface ApplicationReportRow {
  workflowInstanceId: number;
  applicationId: number;
  applicationName: string;
  workflowDefinitionName: string;
  submittedByUserName: string;
  status: WorkflowInstanceStatus;
  currentStepName: string | null;
  startedDate: string;
  completedDate: string | null;
  durationHours: number | null;
}

export interface StatusCount {
  status: WorkflowInstanceStatus;
  count: number;
}

export interface UserTaskReportRow {
  userId: number;
  username: string;
  fullName: string;
  approvedCount: number;
  rejectedCount: number;
  completedCount: number;
  totalActioned: number;
}
