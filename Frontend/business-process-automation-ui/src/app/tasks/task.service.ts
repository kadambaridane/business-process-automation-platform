import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse, PagedResult } from '../core/models/api-response.model';
import { WorkflowTask, WorkflowTaskDetail } from './task.model';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly baseUrl = `${environment.apiUrl}/tasks`;

  constructor(private readonly http: HttpClient) {}

  getMy(page: number, pageSize: number): Observable<ApiResponse<PagedResult<WorkflowTask>>> {
    return this.http.get<ApiResponse<PagedResult<WorkflowTask>>>(`${this.baseUrl}/my`, { params: { page, pageSize } });
  }

  getPending(page: number, pageSize: number): Observable<ApiResponse<PagedResult<WorkflowTask>>> {
    return this.http.get<ApiResponse<PagedResult<WorkflowTask>>>(`${this.baseUrl}/pending`, { params: { page, pageSize } });
  }

  getCompleted(page: number, pageSize: number): Observable<ApiResponse<PagedResult<WorkflowTask>>> {
    return this.http.get<ApiResponse<PagedResult<WorkflowTask>>>(`${this.baseUrl}/completed`, { params: { page, pageSize } });
  }

  getById(taskId: number): Observable<ApiResponse<WorkflowTaskDetail>> {
    return this.http.get<ApiResponse<WorkflowTaskDetail>>(`${this.baseUrl}/${taskId}`);
  }

  approve(taskId: number, comments: string | null): Observable<ApiResponse<WorkflowTaskDetail>> {
    return this.http.post<ApiResponse<WorkflowTaskDetail>>(`${this.baseUrl}/${taskId}/approve`, { comments });
  }

  reject(taskId: number, comments: string | null): Observable<ApiResponse<WorkflowTaskDetail>> {
    return this.http.post<ApiResponse<WorkflowTaskDetail>>(`${this.baseUrl}/${taskId}/reject`, { comments });
  }

  complete(taskId: number, comments: string | null): Observable<ApiResponse<WorkflowTaskDetail>> {
    return this.http.post<ApiResponse<WorkflowTaskDetail>>(`${this.baseUrl}/${taskId}/complete`, { comments });
  }
}
