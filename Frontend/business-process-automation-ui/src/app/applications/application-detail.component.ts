import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner/loading-spinner.component';
import { Application } from './application.model';
import { ApplicationService } from './application.service';

@Component({
  selector: 'app-application-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinnerComponent],
  templateUrl: './application-detail.component.html'
})
export class ApplicationDetailComponent implements OnInit {
  readonly application = signal<Application | null>(null);
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  constructor(
    private readonly route: ActivatedRoute,
    private readonly applicationService: ApplicationService,
    readonly auth: AuthService
  ) {}

  get canManage(): boolean {
    return this.auth.hasRole('Admin', 'Business User');
  }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.applicationService.getById(id).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.application.set(response.data);
        }
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Application not found.');
      }
    });
  }

  toggleStatus(): void {
    const app = this.application();
    if (!app) return;

    this.applicationService.setStatus(app.id, !app.isActive).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.application.set(response.data);
        }
      }
    });
  }
}
