import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner/loading-spinner.component';
import { PaginationComponent } from '../shared/components/pagination/pagination.component';
import { WorkflowTask } from './task.model';
import { TaskService } from './task.service';

type TaskTab = 'my' | 'pending' | 'completed';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinnerComponent, PaginationComponent],
  templateUrl: './task-list.component.html'
})
export class TaskListComponent implements OnInit {
  readonly tasks = signal<WorkflowTask[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  tab: TaskTab = 'my';
  page = 1;
  readonly pageSize = 10;

  constructor(
    private readonly taskService: TaskService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    readonly auth: AuthService
  ) {}

  get canSeePending(): boolean {
    return this.auth.hasRole('Admin', 'Business User');
  }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe((params) => {
      const requestedTab = (params.get('tab') as TaskTab) ?? 'my';
      this.tab = requestedTab === 'pending' && !this.canSeePending ? 'my' : requestedTab;
      this.page = 1;
      this.load();
    });
  }

  switchTab(tab: TaskTab): void {
    this.router.navigate([], { relativeTo: this.route, queryParams: { tab } });
  }

  load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    const request$ =
      this.tab === 'pending'
        ? this.taskService.getPending(this.page, this.pageSize)
        : this.tab === 'completed'
          ? this.taskService.getCompleted(this.page, this.pageSize)
          : this.taskService.getMy(this.page, this.pageSize);

    request$.subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.tasks.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
        }
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Failed to load tasks.');
      }
    });
  }

  onPageChange(page: number): void {
    this.page = page;
    this.load();
  }
}
