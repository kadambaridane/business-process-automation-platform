import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse, PagedResult } from '../core/models/api-response.model';
import { WorkflowInstanceStatus } from '../workflow-instances/workflow-instance.model';
import { ApplicationReportRow, StatusCount, UserTaskReportRow } from './report.model';

export interface ApplicationReportFilters {
  status?: WorkflowInstanceStatus | '';
  applicationId?: number | null;
  dateFrom?: string | null;
  dateTo?: string | null;
  page: number;
  pageSize: number;
}

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly baseUrl = `${environment.apiUrl}/reports`;

  constructor(private readonly http: HttpClient) {}

  getApplicationReport(filters: ApplicationReportFilters): Observable<ApiResponse<PagedResult<ApplicationReportRow>>> {
    const params = this.buildParams(filters);
    return this.http.get<ApiResponse<PagedResult<ApplicationReportRow>>>(`${this.baseUrl}/applications`, { params });
  }

  getStatusSummary(filters: Pick<ApplicationReportFilters, 'applicationId' | 'dateFrom' | 'dateTo'>): Observable<ApiResponse<StatusCount[]>> {
    const params = this.buildParams(filters);
    return this.http.get<ApiResponse<StatusCount[]>>(`${this.baseUrl}/applications/status-summary`, { params });
  }

  getUserTaskReport(dateFrom?: string | null, dateTo?: string | null): Observable<ApiResponse<UserTaskReportRow[]>> {
    const params = this.buildParams({ dateFrom, dateTo });
    return this.http.get<ApiResponse<UserTaskReportRow[]>>(`${this.baseUrl}/user-tasks`, { params });
  }

  private buildParams(filters: Partial<ApplicationReportFilters>): Record<string, string | number> {
    const params: Record<string, string | number> = {};
    if (filters.status) params['status'] = filters.status;
    if (filters.applicationId) params['applicationId'] = filters.applicationId;
    if (filters.dateFrom) params['dateFrom'] = filters.dateFrom;
    if (filters.dateTo) params['dateTo'] = filters.dateTo;
    if (filters.page) params['page'] = filters.page;
    if (filters.pageSize) params['pageSize'] = filters.pageSize;
    return params;
  }
}
