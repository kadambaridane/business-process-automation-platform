import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse, PagedResult } from '../core/models/api-response.model';
import { AppNotification } from './notification.model';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly baseUrl = `${environment.apiUrl}/notifications`;

  /** Shared across the app (bell + notifications page) so both stay in sync without manual wiring. */
  readonly unreadCount = signal(0);

  constructor(private readonly http: HttpClient) {}

  getAll(unreadOnly: boolean, page: number, pageSize: number): Observable<ApiResponse<PagedResult<AppNotification>>> {
    return this.http.get<ApiResponse<PagedResult<AppNotification>>>(this.baseUrl, { params: { unreadOnly, page, pageSize } });
  }

  refreshUnreadCount(): void {
    this.http.get<ApiResponse<number>>(`${this.baseUrl}/unread-count`).subscribe({
      next: (response) => {
        if (response.success && response.data !== null) {
          this.unreadCount.set(response.data);
        }
      }
    });
  }

  markAsRead(id: number): Observable<ApiResponse<object>> {
    return this.http.post<ApiResponse<object>>(`${this.baseUrl}/${id}/read`, {}).pipe(tap(() => this.refreshUnreadCount()));
  }

  markAllAsRead(): Observable<ApiResponse<object>> {
    return this.http.post<ApiResponse<object>>(`${this.baseUrl}/read-all`, {}).pipe(tap(() => this.unreadCount.set(0)));
  }
}
