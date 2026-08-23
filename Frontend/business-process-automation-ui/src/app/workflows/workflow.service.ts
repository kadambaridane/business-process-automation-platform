import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../core/models/api-response.model';
import { SaveWorkflowStepRequest, WorkflowDefinition, WorkflowDefinitionSummary } from './workflow.model';

@Injectable({ providedIn: 'root' })
export class WorkflowService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  getByApplication(applicationId: number): Observable<ApiResponse<WorkflowDefinitionSummary[]>> {
    return this.http.get<ApiResponse<WorkflowDefinitionSummary[]>>(`${this.baseUrl}/applications/${applicationId}/workflows`);
  }

  getById(workflowId: number): Observable<ApiResponse<WorkflowDefinition>> {
    return this.http.get<ApiResponse<WorkflowDefinition>>(`${this.baseUrl}/workflows/${workflowId}`);
  }

  create(applicationId: number, name: string): Observable<ApiResponse<WorkflowDefinition>> {
    return this.http.post<ApiResponse<WorkflowDefinition>>(`${this.baseUrl}/applications/${applicationId}/workflows`, { name });
  }

  rename(workflowId: number, name: string): Observable<ApiResponse<WorkflowDefinition>> {
    return this.http.put<ApiResponse<WorkflowDefinition>>(`${this.baseUrl}/workflows/${workflowId}`, { name });
  }

  setStatus(workflowId: number, isActive: boolean): Observable<ApiResponse<WorkflowDefinition>> {
    return this.http.patch<ApiResponse<WorkflowDefinition>>(`${this.baseUrl}/workflows/${workflowId}/status`, { isActive });
  }

  addStep(workflowId: number, request: SaveWorkflowStepRequest): Observable<ApiResponse<WorkflowDefinition>> {
    return this.http.post<ApiResponse<WorkflowDefinition>>(`${this.baseUrl}/workflows/${workflowId}/steps`, request);
  }

  updateStep(stepId: number, request: SaveWorkflowStepRequest): Observable<ApiResponse<WorkflowDefinition>> {
    return this.http.put<ApiResponse<WorkflowDefinition>>(`${this.baseUrl}/workflows/steps/${stepId}`, request);
  }

  deleteStep(stepId: number): Observable<ApiResponse<WorkflowDefinition>> {
    return this.http.delete<ApiResponse<WorkflowDefinition>>(`${this.baseUrl}/workflows/steps/${stepId}`);
  }

  reorderSteps(workflowId: number, stepIdsInOrder: number[]): Observable<ApiResponse<WorkflowDefinition>> {
    return this.http.post<ApiResponse<WorkflowDefinition>>(`${this.baseUrl}/workflows/${workflowId}/steps/reorder`, { stepIdsInOrder });
  }
}
