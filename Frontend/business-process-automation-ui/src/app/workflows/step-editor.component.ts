import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner/loading-spinner.component';
import { Role } from '../roles/role.model';
import { RoleService } from '../roles/role.service';
import { SaveWorkflowStepRequest, WORKFLOW_STEP_TYPES, WorkflowDefinition, WorkflowStep, WorkflowStepType } from './workflow.model';
import { WorkflowService } from './workflow.service';

@Component({
  selector: 'app-step-editor',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, LoadingSpinnerComponent],
  templateUrl: './step-editor.component.html'
})
export class StepEditorComponent implements OnInit {
  applicationId!: number;
  workflowId!: number;

  readonly workflow = signal<WorkflowDefinition | null>(null);
  readonly roles = signal<Role[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly stepTypes = WORKFLOW_STEP_TYPES;

  showStepEditor = false;
  editingStepId: number | null = null;
  saving = false;

  private readonly fb = inject(FormBuilder);

  readonly stepForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    stepType: ['HumanTask' as WorkflowStepType, Validators.required],
    assigneeRoleId: [null as number | null],
    systemActionKey: ['']
  });

  constructor(
    private readonly route: ActivatedRoute,
    private readonly workflowService: WorkflowService,
    private readonly roleService: RoleService
  ) {}

  get needsAssigneeRole(): boolean {
    const type = this.stepForm.value.stepType;
    return type === 'HumanTask' || type === 'Approval';
  }

  get needsSystemActionKey(): boolean {
    return this.stepForm.value.stepType === 'SystemTask';
  }

  ngOnInit(): void {
    this.applicationId = Number(this.route.snapshot.paramMap.get('id'));
    this.workflowId = Number(this.route.snapshot.paramMap.get('workflowId'));
    this.roleService.getAll().subscribe({ next: (r) => r.data && this.roles.set(r.data) });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.workflowService.getById(this.workflowId).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.workflow.set(response.data);
        }
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Failed to load workflow.');
      }
    });
  }

  toggleStatus(): void {
    const current = this.workflow();
    if (!current) return;
    this.workflowService.setStatus(current.id, !current.isActive).subscribe({ next: (r) => r.data && this.workflow.set(r.data) });
  }

  openAddStep(): void {
    this.editingStepId = null;
    this.stepForm.reset({ stepType: 'HumanTask', assigneeRoleId: null, systemActionKey: '' });
    this.showStepEditor = true;
  }

  openEditStep(step: WorkflowStep): void {
    this.editingStepId = step.id;
    this.stepForm.reset({
      name: step.name,
      stepType: step.stepType,
      assigneeRoleId: step.assigneeRoleId,
      systemActionKey: step.systemActionKey ?? ''
    });
    this.showStepEditor = true;
  }

  cancelStepEditor(): void {
    this.showStepEditor = false;
    this.editingStepId = null;
    this.errorMessage.set(null);
  }

  saveStep(): void {
    if (this.stepForm.invalid) {
      this.stepForm.markAllAsTouched();
      return;
    }

    const value = this.stepForm.getRawValue();
    const request: SaveWorkflowStepRequest = {
      name: value.name!,
      stepType: value.stepType!,
      assigneeRoleId: this.needsAssigneeRole ? value.assigneeRoleId : null,
      systemActionKey: this.needsSystemActionKey ? value.systemActionKey || null : null
    };

    this.saving = true;
    this.errorMessage.set(null);
    const save$ = this.editingStepId
      ? this.workflowService.updateStep(this.editingStepId, request)
      : this.workflowService.addStep(this.workflowId, request);

    save$.subscribe({
      next: (response) => {
        this.saving = false;
        this.showStepEditor = false;
        if (response.success && response.data) {
          this.workflow.set(response.data);
        }
      },
      error: (err) => {
        this.saving = false;
        this.errorMessage.set(err.error?.Message ?? 'Failed to save step.');
      }
    });
  }

  deleteStep(step: WorkflowStep): void {
    this.workflowService.deleteStep(step.id).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.workflow.set(response.data);
        }
      },
      error: () => this.errorMessage.set('Failed to delete step.')
    });
  }

  moveStep(step: WorkflowStep, direction: -1 | 1): void {
    const current = this.workflow();
    if (!current) return;

    const sorted = [...current.steps].sort((a, b) => a.stepOrder - b.stepOrder);
    const index = sorted.findIndex((s) => s.id === step.id);
    const targetIndex = index + direction;
    if (targetIndex < 0 || targetIndex >= sorted.length) return;

    [sorted[index], sorted[targetIndex]] = [sorted[targetIndex], sorted[index]];

    this.workflowService.reorderSteps(this.workflowId, sorted.map((s) => s.id)).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.workflow.set(response.data);
        }
      },
      error: () => this.errorMessage.set('Failed to reorder steps.')
    });
  }
}
