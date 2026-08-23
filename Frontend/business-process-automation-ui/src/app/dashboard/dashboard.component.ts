import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner/loading-spinner.component';
import { DashboardSummary } from './dashboard.model';
import { DashboardService } from './dashboard.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinnerComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  readonly summary = signal<DashboardSummary | null>(null);
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  constructor(
    readonly auth: AuthService,
    private readonly dashboardService: DashboardService
  ) {}

  ngOnInit(): void {
    this.dashboardService.getSummary().subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.summary.set(response.data);
        }
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Failed to load dashboard data.');
      }
    });
  }
}
