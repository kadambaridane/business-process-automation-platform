import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApplicationService } from './application.service';

@Component({
  selector: 'app-application-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './application-form.component.html'
})
export class ApplicationFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly applicationService = inject(ApplicationService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['']
  });

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  applicationId: number | null = null;

  get isEditMode(): boolean {
    return this.applicationId !== null;
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.applicationId = Number(idParam);
      this.loadApplication(this.applicationId);
    }
  }

  private loadApplication(id: number): void {
    this.loading.set(true);
    this.applicationService.getById(id).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.form.patchValue({
            name: response.data.name,
            description: response.data.description
          });
        }
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Failed to load application.');
      }
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.errorMessage.set(null);

    const request = { name: this.form.value.name!, description: this.form.value.description || null };
    const save$ = this.isEditMode
      ? this.applicationService.update(this.applicationId!, request)
      : this.applicationService.create(request);

    save$.subscribe({
      next: (response) => {
        this.saving.set(false);
        if (response.success && response.data) {
          this.router.navigate(['/applications', response.data.id]);
        }
      },
      error: () => {
        this.saving.set(false);
        this.errorMessage.set('Failed to save application.');
      }
    });
  }
}
