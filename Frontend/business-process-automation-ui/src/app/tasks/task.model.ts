import { WorkflowStepType } from '../workflows/workflow.model';

export type WorkflowTaskStatus = 'Pending' | 'Approved' | 'Rejected' | 'Completed';

export interface WorkflowTask {
  id: number;
  title: string;
  workflowInstanceId: number;
  applicationId: number;
  applicationName: string;
  stepType: WorkflowStepType;
  assignedRoleName: string | null;
  assignedToUserName: string | null;
  status: WorkflowTaskStatus;
  createdDate: string;
  completedDate: string | null;
}

export interface TaskHistoryEntry {
  id: number;
  action: string;
  actionByUserName: string;
  comments: string | null;
  actionDate: string;
}

export interface WorkflowTaskDetail extends WorkflowTask {
  comments: string | null;
  submissionData: string;
  history: TaskHistoryEntry[];
}
