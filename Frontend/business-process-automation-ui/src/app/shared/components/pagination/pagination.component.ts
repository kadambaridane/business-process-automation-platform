import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="pagination" *ngIf="totalPages > 1">
      <button type="button" [disabled]="page <= 1" (click)="go(page - 1)">Previous</button>
      <span>Page {{ page }} of {{ totalPages }}</span>
      <button type="button" [disabled]="page >= totalPages" (click)="go(page + 1)">Next</button>
    </div>
  `,
  styles: [
    `
      .pagination {
        display: flex;
        align-items: center;
        gap: 12px;
        justify-content: flex-end;
        margin-top: 16px;
        font-size: 0.9rem;
      }
      button {
        padding: 6px 14px;
        border: 1px solid #cbd2e0;
        background: #fff;
        border-radius: 4px;
        cursor: pointer;
      }
      button:disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }
    `
  ]
})
export class PaginationComponent {
  @Input() page = 1;
  @Input() totalCount = 0;
  @Input() pageSize = 20;
  @Output() pageChange = new EventEmitter<number>();

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  go(page: number): void {
    this.pageChange.emit(page);
  }
}
