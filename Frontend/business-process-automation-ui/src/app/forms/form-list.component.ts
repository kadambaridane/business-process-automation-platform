import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner/loading-spinner.component';
import { FormDefinitionSummary } from './form.model';
import { FormService } from './form.service';

@Component({
  selector: 'app-form-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, LoadingSpinnerComponent],
  templateUrl: './form-list.component.html'
})
export class FormListComponent implements OnInit {
  applicationId!: number;
  readonly forms = signal<FormDefinitionSummary[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  newFormName = '';
  creating = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly formService: FormService
  ) {}

  ngOnInit(): void {
    this.applicationId = Number(this.route.snapshot.paramMap.get('id'));
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.formService.getByApplication(this.applicationId).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.forms.set(response.data);
        }
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Failed to load forms.');
      }
    });
  }

  createForm(): void {
    if (!this.newFormName.trim()) {
      return;
    }
    this.creating = true;
    this.formService.create(this.applicationId, this.newFormName.trim()).subscribe({
      next: () => {
        this.creating = false;
        this.newFormName = '';
        this.load();
      },
      error: () => {
        this.creating = false;
        this.errorMessage.set('Failed to create form.');
      }
    });
  }

  toggleStatus(form: FormDefinitionSummary): void {
    this.formService.setStatus(form.id, !form.isActive).subscribe({
      next: () => this.load(),
      error: () => this.errorMessage.set('Failed to update form status.')
    });
  }
}
