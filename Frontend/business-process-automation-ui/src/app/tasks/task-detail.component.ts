import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner/loading-spinner.component';
import { WorkflowTaskDetail } from './task.model';
import { TaskService } from './task.service';

@Component({
  selector: 'app-task-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, LoadingSpinnerComponent],
  templateUrl: './task-detail.component.html'
})
export class TaskDetailComponent implements OnInit {
  taskId!: number;
  readonly task = signal<WorkflowTaskDetail | null>(null);
  readonly loading = signal(true);
  readonly acting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  comments = '';

  constructor(
    private readonly route: ActivatedRoute,
    private readonly taskService: TaskService
  ) {}

  get submissionEntries(): [string, unknown][] {
    const data = this.task()?.submissionData;
    if (!data) return [];
    try {
      return Object.entries(JSON.parse(data));
    } catch {
      return [];
    }
  }

  ngOnInit(): void {
    this.taskId = Number(this.route.snapshot.paramMap.get('id'));
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.taskService.getById(this.taskId).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.task.set(response.data);
        }
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Failed to load task.');
      }
    });
  }

  act(action: 'approve' | 'reject' | 'complete'): void {
    this.acting.set(true);
    this.errorMessage.set(null);

    const request$ =
      action === 'approve'
        ? this.taskService.approve(this.taskId, this.comments || null)
        : action === 'reject'
          ? this.taskService.reject(this.taskId, this.comments || null)
          : this.taskService.complete(this.taskId, this.comments || null);

    request$.subscribe({
      next: (response) => {
        this.acting.set(false);
        if (response.success && response.data) {
          this.task.set(response.data);
          this.comments = '';
        }
      },
      error: (err) => {
        this.acting.set(false);
        this.errorMessage.set(err.error?.Message ?? 'Failed to act on this task.');
      }
    });
  }
}
