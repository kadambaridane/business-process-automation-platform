import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse, PagedResult } from '../core/models/api-response.model';
import { Application, SaveApplicationRequest } from './application.model';

@Injectable({ providedIn: 'root' })
export class ApplicationService {
  private readonly baseUrl = `${environment.apiUrl}/applications`;

  constructor(private readonly http: HttpClient) {}

  getAll(params: { search?: string; isActive?: boolean; page: number; pageSize: number }): Observable<ApiResponse<PagedResult<Application>>> {
    let httpParams = new HttpParams().set('page', params.page).set('pageSize', params.pageSize);
    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }
    if (params.isActive !== undefined) {
      httpParams = httpParams.set('isActive', params.isActive);
    }
    return this.http.get<ApiResponse<PagedResult<Application>>>(this.baseUrl, { params: httpParams });
  }

  getById(id: number): Observable<ApiResponse<Application>> {
    return this.http.get<ApiResponse<Application>>(`${this.baseUrl}/${id}`);
  }

  create(request: SaveApplicationRequest): Observable<ApiResponse<Application>> {
    return this.http.post<ApiResponse<Application>>(this.baseUrl, request);
  }

  update(id: number, request: SaveApplicationRequest): Observable<ApiResponse<Application>> {
    return this.http.put<ApiResponse<Application>>(`${this.baseUrl}/${id}`, request);
  }

  setStatus(id: number, isActive: boolean): Observable<ApiResponse<Application>> {
    return this.http.patch<ApiResponse<Application>>(`${this.baseUrl}/${id}/status`, { isActive });
  }
}
