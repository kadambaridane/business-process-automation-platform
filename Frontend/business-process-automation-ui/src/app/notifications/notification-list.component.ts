import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner/loading-spinner.component';
import { PaginationComponent } from '../shared/components/pagination/pagination.component';
import { AppNotification } from './notification.model';
import { NotificationService } from './notification.service';

@Component({
  selector: 'app-notification-list',
  standalone: true,
  imports: [CommonModule, FormsModule, LoadingSpinnerComponent, PaginationComponent],
  templateUrl: './notification-list.component.html'
})
export class NotificationListComponent implements OnInit {
  readonly notifications = signal<AppNotification[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  unreadOnly = false;
  page = 1;
  readonly pageSize = 15;

  constructor(private readonly notificationService: NotificationService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.notificationService.getAll(this.unreadOnly, this.page, this.pageSize).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.notifications.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
        }
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Failed to load notifications.');
      }
    });
  }

  onFilterChange(): void {
    this.page = 1;
    this.load();
  }

  onPageChange(page: number): void {
    this.page = page;
    this.load();
  }

  markAsRead(notification: AppNotification): void {
    if (notification.isRead) return;
    this.notificationService.markAsRead(notification.id).subscribe({
      next: () => (notification.isRead = true)
    });
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe({ next: () => this.load() });
  }
}
