import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Application } from '../applications/application.model';
import { ApplicationService } from '../applications/application.service';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner/loading-spinner.component';
import { PaginationComponent } from '../shared/components/pagination/pagination.component';
import { WorkflowInstanceStatus } from '../workflow-instances/workflow-instance.model';
import { ApplicationReportRow, StatusCount, UserTaskReportRow } from './report.model';
import { ReportService } from './report.service';

type ReportTab = 'applications' | 'user-tasks';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, LoadingSpinnerComponent, PaginationComponent],
  templateUrl: './reports.component.html'
})
export class ReportsComponent implements OnInit {
  tab: ReportTab = 'applications';

  readonly applications = signal<Application[]>([]);
  readonly reportRows = signal<ApplicationReportRow[]>([]);
  readonly statusSummary = signal<StatusCount[]>([]);
  readonly userTaskRows = signal<UserTaskReportRow[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(false);

  statusFilter: WorkflowInstanceStatus | '' = '';
  applicationIdFilter: number | null = null;
  dateFrom = '';
  dateTo = '';
  page = 1;
  readonly pageSize = 10;

  constructor(
    private readonly reportService: ReportService,
    private readonly applicationService: ApplicationService
  ) {}

  ngOnInit(): void {
    this.applicationService.getAll({ page: 1, pageSize: 100 }).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.applications.set(response.data.items);
        }
      }
    });

    this.loadApplicationReport();
  }

  switchTab(tab: ReportTab): void {
    this.tab = tab;
    if (tab === 'user-tasks') {
      this.loadUserTaskReport();
    } else {
      this.loadApplicationReport();
    }
  }

  applyFilters(): void {
    this.page = 1;
    this.loadApplicationReport();
  }

  onPageChange(page: number): void {
    this.page = page;
    this.loadApplicationReport();
  }

  private loadApplicationReport(): void {
    this.loading.set(true);

    const filters = {
      status: this.statusFilter,
      applicationId: this.applicationIdFilter,
      dateFrom: this.dateFrom || null,
      dateTo: this.dateTo || null,
      page: this.page,
      pageSize: this.pageSize
    };

    this.reportService.getApplicationReport(filters).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.reportRows.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
        }
      },
      error: () => this.loading.set(false)
    });

    this.reportService
      .getStatusSummary({ applicationId: this.applicationIdFilter, dateFrom: this.dateFrom || null, dateTo: this.dateTo || null })
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.statusSummary.set(response.data);
          }
        }
      });
  }

  private loadUserTaskReport(): void {
    this.loading.set(true);
    this.reportService.getUserTaskReport(this.dateFrom || null, this.dateTo || null).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.userTaskRows.set(response.data);
        }
      },
      error: () => this.loading.set(false)
    });
  }

  countFor(status: WorkflowInstanceStatus): number {
    return this.statusSummary().find((s) => s.status === status)?.count ?? 0;
  }
}
