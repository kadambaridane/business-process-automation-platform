import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner/loading-spinner.component';
import { WorkflowInstance, WorkflowStepInstance } from './workflow-instance.model';
import { WorkflowInstanceService } from './workflow-instance.service';

@Component({
  selector: 'app-workflow-instance-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinnerComponent],
  templateUrl: './workflow-instance-detail.component.html'
})
export class WorkflowInstanceDetailComponent implements OnInit {
  readonly instance = signal<WorkflowInstance | null>(null);
  readonly history = signal<WorkflowStepInstance[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  constructor(
    private readonly route: ActivatedRoute,
    private readonly workflowInstanceService: WorkflowInstanceService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    this.workflowInstanceService.getById(id).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.instance.set(response.data);
        }
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Failed to load workflow instance.');
      }
    });

    this.workflowInstanceService.getHistory(id).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.history.set(response.data);
        }
      }
    });
  }
}
