import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DynamicFormComponent } from '../shared/components/dynamic-form/dynamic-form.component';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner/loading-spinner.component';
import { FormDefinition, FormSubmission } from './form.model';
import { FormService } from './form.service';

@Component({
  selector: 'app-form-submit',
  standalone: true,
  imports: [CommonModule, RouterLink, DynamicFormComponent, LoadingSpinnerComponent],
  templateUrl: './form-submit.component.html'
})
export class FormSubmitComponent implements OnInit {
  applicationId!: number;
  readonly activeForm = signal<FormDefinition | null>(null);
  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly submittedResult = signal<FormSubmission | null>(null);

  constructor(
    private readonly route: ActivatedRoute,
    private readonly formService: FormService
  ) {}

  ngOnInit(): void {
    this.applicationId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadActiveForm();
  }

  private loadActiveForm(): void {
    this.loading.set(true);
    this.formService.getByApplication(this.applicationId).subscribe({
      next: (response) => {
        const active = response.data?.find((f) => f.isActive);
        if (!active) {
          this.loading.set(false);
          this.errorMessage.set('This application has no active form to submit.');
          return;
        }
        this.formService.getById(active.id).subscribe({
          next: (formResponse) => {
            this.loading.set(false);
            if (formResponse.success && formResponse.data) {
              this.activeForm.set(formResponse.data);
            }
          },
          error: () => {
            this.loading.set(false);
            this.errorMessage.set('Failed to load the form.');
          }
        });
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Failed to load the form.');
      }
    });
  }

  onSubmit(values: Record<string, unknown>): void {
    const form = this.activeForm();
    if (!form) return;

    this.submitting.set(true);
    this.errorMessage.set(null);

    this.formService.submit(form.id, values).subscribe({
      next: (response) => {
        this.submitting.set(false);
        if (response.success && response.data) {
          this.submittedResult.set(response.data);
        }
      },
      error: (err) => {
        this.submitting.set(false);
        this.errorMessage.set(err.error?.Message ?? 'Submission failed. Please check your entries.');
      }
    });
  }
}
