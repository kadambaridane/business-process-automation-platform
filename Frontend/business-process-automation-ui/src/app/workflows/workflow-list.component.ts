import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner/loading-spinner.component';
import { WorkflowDefinitionSummary } from './workflow.model';
import { WorkflowService } from './workflow.service';

@Component({
  selector: 'app-workflow-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, LoadingSpinnerComponent],
  templateUrl: './workflow-list.component.html'
})
export class WorkflowListComponent implements OnInit {
  applicationId!: number;
  readonly workflows = signal<WorkflowDefinitionSummary[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  newWorkflowName = '';
  creating = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly workflowService: WorkflowService
  ) {}

  ngOnInit(): void {
    this.applicationId = Number(this.route.snapshot.paramMap.get('id'));
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.workflowService.getByApplication(this.applicationId).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.workflows.set(response.data);
        }
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Failed to load workflows.');
      }
    });
  }

  createWorkflow(): void {
    if (!this.newWorkflowName.trim()) {
      return;
    }
    this.creating = true;
    this.workflowService.create(this.applicationId, this.newWorkflowName.trim()).subscribe({
      next: () => {
        this.creating = false;
        this.newWorkflowName = '';
        this.load();
      },
      error: () => {
        this.creating = false;
        this.errorMessage.set('Failed to create workflow.');
      }
    });
  }

  toggleStatus(workflow: WorkflowDefinitionSummary): void {
    this.workflowService.setStatus(workflow.id, !workflow.isActive).subscribe({
      next: () => this.load(),
      error: () => this.errorMessage.set('Failed to update workflow status.')
    });
  }
}
