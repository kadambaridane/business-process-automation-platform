import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { interval, startWith } from 'rxjs';
import { AppNotification } from './notification.model';
import { NotificationService } from './notification.service';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './notification-bell.component.html',
  styleUrl: './notification-bell.component.css'
})
export class NotificationBellComponent implements OnInit {
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  readonly unreadCount = this.notificationService.unreadCount;
  readonly recentNotifications = signal<AppNotification[]>([]);
  readonly panelOpen = signal(false);

  ngOnInit(): void {
    interval(30000)
      .pipe(startWith(0), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.notificationService.refreshUnreadCount());
  }

  togglePanel(): void {
    this.panelOpen.set(!this.panelOpen());
    if (this.panelOpen()) {
      this.loadRecent();
    }
  }

  closePanel(): void {
    this.panelOpen.set(false);
  }

  markAsRead(notification: AppNotification): void {
    if (notification.isRead) return;
    this.notificationService.markAsRead(notification.id).subscribe({
      next: () => (notification.isRead = true)
    });
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe({
      next: () => this.recentNotifications.update((items) => items.map((n) => ({ ...n, isRead: true })))
    });
  }

  private loadRecent(): void {
    this.notificationService.getAll(false, 1, 8).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.recentNotifications.set(response.data.items);
        }
      }
    });
  }
}
