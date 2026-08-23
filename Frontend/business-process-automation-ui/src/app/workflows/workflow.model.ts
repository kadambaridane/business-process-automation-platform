export type WorkflowStepType = 'HumanTask' | 'SystemTask' | 'Approval' | 'Decision' | 'Notification' | 'End';

export const WORKFLOW_STEP_TYPES: WorkflowStepType[] = [
  'HumanTask',
  'SystemTask',
  'Approval',
  'Decision',
  'Notification',
  'End'
];

export interface WorkflowStep {
  id: number;
  name: string;
  stepOrder: number;
  stepType: WorkflowStepType;
  assigneeRoleId: number | null;
  assigneeRoleName: string | null;
  systemActionKey: string | null;
}

export interface SaveWorkflowStepRequest {
  name: string;
  stepType: WorkflowStepType;
  stepOrder?: number | null;
  assigneeRoleId?: number | null;
  systemActionKey?: string | null;
}

export interface WorkflowDefinition {
  id: number;
  name: string;
  isActive: boolean;
  applicationId: number;
  createdDate: string;
  steps: WorkflowStep[];
}

export interface WorkflowDefinitionSummary {
  id: number;
  name: string;
  isActive: boolean;
  stepCount: number;
  createdDate: string;
}
