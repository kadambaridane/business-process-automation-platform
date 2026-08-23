export interface Application {
  id: number;
  name: string;
  description: string | null;
  isActive: boolean;
  createdByUserName: string;
  createdDate: string;
  formDefinitionCount: number;
  workflowDefinitionCount: number;
}

export interface SaveApplicationRequest {
  name: string;
  description: string | null;
}
