import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner/loading-spinner.component';
import { PaginationComponent } from '../shared/components/pagination/pagination.component';
import { Application } from './application.model';
import { ApplicationService } from './application.service';

@Component({
  selector: 'app-application-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, LoadingSpinnerComponent, PaginationComponent],
  templateUrl: './application-list.component.html'
})
export class ApplicationListComponent implements OnInit {
  readonly applications = signal<Application[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  search = '';
  statusFilter: 'all' | 'active' | 'inactive' = 'all';
  page = 1;
  readonly pageSize = 10;

  constructor(
    private readonly applicationService: ApplicationService,
    readonly auth: AuthService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  get canManage(): boolean {
    return this.auth.hasRole('Admin', 'Business User');
  }

  load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    const isActive = this.statusFilter === 'all' ? undefined : this.statusFilter === 'active';

    this.applicationService.getAll({ search: this.search, isActive, page: this.page, pageSize: this.pageSize }).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.applications.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
        }
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Failed to load applications.');
      }
    });
  }

  onSearchChange(): void {
    this.page = 1;
    this.load();
  }

  onPageChange(page: number): void {
    this.page = page;
    this.load();
  }

  toggleStatus(app: Application): void {
    this.applicationService.setStatus(app.id, !app.isActive).subscribe({
      next: () => this.load(),
      error: () => this.errorMessage.set('Failed to update application status.')
    });
  }
}
