import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse, PagedResult } from '../core/models/api-response.model';
import { FormDefinition, FormDefinitionSummary, FormSubmission, SaveFormFieldRequest } from './form.model';

@Injectable({ providedIn: 'root' })
export class FormService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  getByApplication(applicationId: number): Observable<ApiResponse<FormDefinitionSummary[]>> {
    return this.http.get<ApiResponse<FormDefinitionSummary[]>>(`${this.baseUrl}/applications/${applicationId}/forms`);
  }

  getById(formId: number): Observable<ApiResponse<FormDefinition>> {
    return this.http.get<ApiResponse<FormDefinition>>(`${this.baseUrl}/forms/${formId}`);
  }

  create(applicationId: number, name: string): Observable<ApiResponse<FormDefinition>> {
    return this.http.post<ApiResponse<FormDefinition>>(`${this.baseUrl}/applications/${applicationId}/forms`, { name });
  }

  rename(formId: number, name: string): Observable<ApiResponse<FormDefinition>> {
    return this.http.put<ApiResponse<FormDefinition>>(`${this.baseUrl}/forms/${formId}`, { name });
  }

  setStatus(formId: number, isActive: boolean): Observable<ApiResponse<FormDefinition>> {
    return this.http.patch<ApiResponse<FormDefinition>>(`${this.baseUrl}/forms/${formId}/status`, { isActive });
  }

  addField(formId: number, request: SaveFormFieldRequest): Observable<ApiResponse<FormDefinition>> {
    return this.http.post<ApiResponse<FormDefinition>>(`${this.baseUrl}/forms/${formId}/fields`, request);
  }

  updateField(fieldId: number, request: SaveFormFieldRequest): Observable<ApiResponse<FormDefinition>> {
    return this.http.put<ApiResponse<FormDefinition>>(`${this.baseUrl}/forms/fields/${fieldId}`, request);
  }

  deleteField(fieldId: number): Observable<ApiResponse<FormDefinition>> {
    return this.http.delete<ApiResponse<FormDefinition>>(`${this.baseUrl}/forms/fields/${fieldId}`);
  }

  reorderFields(formId: number, fieldIdsInOrder: number[]): Observable<ApiResponse<FormDefinition>> {
    return this.http.post<ApiResponse<FormDefinition>>(`${this.baseUrl}/forms/${formId}/fields/reorder`, { fieldIdsInOrder });
  }

  submit(formId: number, values: Record<string, unknown>): Observable<ApiResponse<FormSubmission>> {
    return this.http.post<ApiResponse<FormSubmission>>(`${this.baseUrl}/forms/${formId}/submissions`, { values });
  }

  getSubmission(submissionId: number): Observable<ApiResponse<FormSubmission>> {
    return this.http.get<ApiResponse<FormSubmission>>(`${this.baseUrl}/forms/submissions/${submissionId}`);
  }

  getSubmissions(formId: number, page: number, pageSize: number): Observable<ApiResponse<PagedResult<FormSubmission>>> {
    return this.http.get<ApiResponse<PagedResult<FormSubmission>>>(`${this.baseUrl}/forms/${formId}/submissions`, {
      params: { page, pageSize }
    });
  }
}
