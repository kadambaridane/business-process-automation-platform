import { Component } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  template: `<div class="spinner" role="status" aria-label="Loading"></div>`,
  styles: [
    `
      .spinner {
        width: 28px;
        height: 28px;
        border: 3px solid #d8dee8;
        border-top-color: #2f5fdb;
        border-radius: 50%;
        margin: 24px auto;
        animation: spin 0.8s linear infinite;
      }
      @keyframes spin {
        to {
          transform: rotate(360deg);
        }
      }
    `
  ]
})
export class LoadingSpinnerComponent {}
