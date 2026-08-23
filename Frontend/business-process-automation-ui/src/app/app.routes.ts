import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';
import { ShellComponent } from './core/layout/shell.component';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./auth/login.component').then((m) => m.LoginComponent)
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () => import('./dashboard/dashboard.component').then((m) => m.DashboardComponent)
      },
      {
        path: 'applications',
        loadComponent: () => import('./applications/application-list.component').then((m) => m.ApplicationListComponent)
      },
      {
        path: 'applications/new',
        loadComponent: () => import('./applications/application-form.component').then((m) => m.ApplicationFormComponent)
      },
      {
        path: 'applications/:id',
        loadComponent: () => import('./applications/application-detail.component').then((m) => m.ApplicationDetailComponent)
      },
      {
        path: 'applications/:id/edit',
        loadComponent: () => import('./applications/application-form.component').then((m) => m.ApplicationFormComponent)
      },
      {
        path: 'applications/:id/forms',
        loadComponent: () => import('./forms/form-list.component').then((m) => m.FormListComponent)
      },
      {
        path: 'applications/:id/forms/:formId',
        loadComponent: () => import('./forms/field-editor.component').then((m) => m.FieldEditorComponent)
      },
      {
        path: 'applications/:id/submit',
        loadComponent: () => import('./forms/form-submit.component').then((m) => m.FormSubmitComponent)
      },
      {
        path: 'applications/:id/workflows',
        loadComponent: () => import('./workflows/workflow-list.component').then((m) => m.WorkflowListComponent)
      },
      {
        path: 'applications/:id/workflows/:workflowId',
        loadComponent: () => import('./workflows/step-editor.component').then((m) => m.StepEditorComponent)
      },
      {
        path: 'tasks',
        loadComponent: () => import('./tasks/task-list.component').then((m) => m.TaskListComponent)
      },
      {
        path: 'tasks/:id',
        loadComponent: () => import('./tasks/task-detail.component').then((m) => m.TaskDetailComponent)
      },
      {
        path: 'workflow-instances/:id',
        loadComponent: () =>
          import('./workflow-instances/workflow-instance-detail.component').then((m) => m.WorkflowInstanceDetailComponent)
      },
      {
        path: 'notifications',
        loadComponent: () => import('./notifications/notification-list.component').then((m) => m.NotificationListComponent)
      },
      {
        path: 'reports',
        canActivate: [roleGuard('Admin', 'Business User')],
        loadComponent: () => import('./reports/reports.component').then((m) => m.ReportsComponent)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
