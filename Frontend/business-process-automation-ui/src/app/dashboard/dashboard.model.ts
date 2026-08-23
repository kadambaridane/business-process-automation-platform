import { WorkflowInstanceStatus } from '../workflow-instances/workflow-instance.model';

export interface RecentApplication {
  id: number;
  name: string;
  isActive: boolean;
  createdByUserName: string;
  createdDate: string;
}

export interface RecentWorkflowActivity {
  workflowInstanceId: number;
  applicationName: string;
  workflowDefinitionName: string;
  submittedByUserName: string;
  status: WorkflowInstanceStatus;
  currentStepName: string | null;
  activityDate: string;
}

export interface DashboardSummary {
  totalApplications: number;
  pendingApplications: number;
  approvedApplications: number;
  rejectedApplications: number;
  pendingTasks: number;
  completedTasks: number;
  recentApplications: RecentApplication[];
  recentWorkflowActivity: RecentWorkflowActivity[];
}
