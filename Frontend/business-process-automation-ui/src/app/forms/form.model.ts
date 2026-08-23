export type FieldType = 'Text' | 'Number' | 'Email' | 'Date' | 'Dropdown' | 'Radio' | 'Checkbox' | 'Textarea';

export const FIELD_TYPES: FieldType[] = ['Text', 'Number', 'Email', 'Date', 'Dropdown', 'Radio', 'Checkbox', 'Textarea'];

export interface FieldValidationRules {
  minLength?: number | null;
  maxLength?: number | null;
  min?: number | null;
  max?: number | null;
  pattern?: string | null;
  options?: string[] | null;
}

export interface FormField {
  id: number;
  fieldName: string;
  label: string;
  fieldType: FieldType;
  isRequired: boolean;
  displayOrder: number;
  defaultValue: string | null;
  validationRules: FieldValidationRules | null;
}

export interface SaveFormFieldRequest {
  fieldName: string;
  label: string;
  fieldType: FieldType;
  isRequired: boolean;
  displayOrder?: number | null;
  defaultValue?: string | null;
  validationRules?: FieldValidationRules | null;
}

export interface FormDefinition {
  id: number;
  name: string;
  isActive: boolean;
  applicationId: number;
  createdDate: string;
  fields: FormField[];
}

export interface FormDefinitionSummary {
  id: number;
  name: string;
  isActive: boolean;
  fieldCount: number;
  createdDate: string;
}

export interface FormSubmission {
  id: number;
  formDefinitionId: number;
  formDefinitionName: string;
  applicationId: number;
  applicationName: string;
  submittedByUserName: string;
  submissionData: string;
  submittedDate: string;
  workflowInstanceId: number | null;
}
