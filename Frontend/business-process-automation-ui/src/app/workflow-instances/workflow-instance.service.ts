import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../core/models/api-response.model';
import { WorkflowInstance, WorkflowStepInstance } from './workflow-instance.model';

@Injectable({ providedIn: 'root' })
export class WorkflowInstanceService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  getById(instanceId: number): Observable<ApiResponse<WorkflowInstance>> {
    return this.http.get<ApiResponse<WorkflowInstance>>(`${this.baseUrl}/workflow-instances/${instanceId}`);
  }

  getHistory(instanceId: number): Observable<ApiResponse<WorkflowStepInstance[]>> {
    return this.http.get<ApiResponse<WorkflowStepInstance[]>>(`${this.baseUrl}/workflow-instances/${instanceId}/history`);
  }
}
