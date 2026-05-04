import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ShiftApiService, Shift } from '../../../core/services/shift-api.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-shift-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    @if (isOpen) {
      <div class="modal-overlay">
        <div class="modal-content glass-panel animate-fade-up">
          
          @if (mode === 'Open') {
            <h2 class="modal-title">Open Register</h2>
            <p class="modal-desc">Please enter the starting cash float in your drawer to begin processing transactions.</p>
            
            <div class="field">
              <label>Starting Float (₹)</label>
              <input type="number" [(ngModel)]="floatAmount" min="0" step="100" />
            </div>
            
            <div class="modal-actions">
              @if (canCancel) {
                <button class="btn btn-secondary" (click)="close.emit()">Cancel</button>
              }
              <button class="btn btn-primary" (click)="submitOpen()" [disabled]="loading()">Open Shift</button>
            </div>
          } 
          @else if (mode === 'Close') {
            <h2 class="modal-title">Close Register (Z-Report)</h2>
            <p class="modal-desc">Count the physical cash in your drawer and enter it below to reconcile your shift.</p>
            
            <div class="stats-grid">
              <div class="stat-box">
                <span class="stat-label">Starting Float</span>
                <strong class="stat-value">₹{{ currentShift?.startingFloat }}</strong>
              </div>
            </div>
            
            <div class="field" style="margin-top: 1.5rem">
              <label>Actual Cash Counted (₹)</label>
              <input type="number" [(ngModel)]="actualCash" min="0" step="10" />
            </div>
            
            <div class="modal-actions">
              <button class="btn btn-secondary" (click)="close.emit()">Cancel</button>
              <button class="btn btn-primary" (click)="submitClose()" [disabled]="loading()">Close Shift</button>
            </div>
          }
          @else if (mode === 'Result') {
            <h2 class="modal-title">Shift Closed Successfully</h2>
            
            <div class="z-report">
              <div class="z-row"><span>Starting Float:</span> <strong>₹{{ zResult?.startingFloat }}</strong></div>
              <div class="z-row"><span>System Expected Cash:</span> <strong>₹{{ zResult?.expectedCash }}</strong></div>
              <div class="z-row"><span>Actual Cash Counted:</span> <strong>₹{{ zResult?.actualCash }}</strong></div>
              <hr style="margin: 0.5rem 0; border: none; border-top: 1px dashed var(--border-color)" />
              <div class="z-row" [class.overage]="zResult?.variance > 0" [class.shortage]="zResult?.variance < 0">
                <span>Variance (Over/Short):</span> 
                <strong>₹{{ zResult?.variance }}</strong>
              </div>
            </div>
            
            <div class="modal-actions">
              <button class="btn btn-primary" (click)="close.emit()" style="width: 100%">Acknowledge & Sign Out</button>
            </div>
          }
        </div>
      </div>
    }
  `,
  styles: [`
    .modal-overlay {
      position: fixed;
      inset: 0;
      background: rgba(0,0,0,0.6);
      backdrop-filter: blur(4px);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 9999;
    }
    .modal-content {
      background: white;
      padding: 2rem;
      border-radius: var(--radius-md);
      width: 100%;
      max-width: 400px;
      box-shadow: var(--shadow-lg);
    }
    .modal-title { margin-top: 0; font-size: 1.25rem; }
    .modal-desc { color: var(--ink-2); font-size: 0.9rem; margin-bottom: 1.5rem; }
    .field label { display: block; font-size: 0.8rem; font-weight: 600; margin-bottom: 0.25rem; color: var(--ink-2); }
    .field input { width: 100%; padding: 0.75rem; border: var(--border-medium); border-radius: var(--radius-sm); font-size: 1rem; }
    .modal-actions { display: flex; gap: 1rem; margin-top: 2rem; justify-content: flex-end; }
    .modal-actions .btn { flex: 1; }
    
    .stats-grid { display: grid; grid-template-columns: 1fr; gap: 1rem; }
    .stat-box { background: rgba(0,0,0,0.03); padding: 1rem; border-radius: var(--radius-sm); display: flex; flex-direction: column; }
    .stat-label { font-size: 0.75rem; color: var(--ink-2); text-transform: uppercase; letter-spacing: 0.5px; }
    .stat-value { font-size: 1.25rem; font-family: monospace; margin-top: 0.25rem; }
    
    .z-report { background: rgba(0,0,0,0.02); padding: 1.5rem; border-radius: var(--radius-sm); font-family: monospace; font-size: 0.95rem; }
    .z-row { display: flex; justify-content: space-between; margin-bottom: 0.5rem; }
    .overage { color: var(--success); }
    .shortage { color: var(--danger); }
  `]
})
export class ShiftModalComponent {
  @Input() isOpen = false;
  @Input() mode: 'Open' | 'Close' | 'Result' = 'Open';
  @Input() canCancel = false;
  @Input() currentShift: Shift | null = null;
  
  @Output() close = new EventEmitter<void>();
  @Output() shiftOpened = new EventEmitter<number>();
  @Output() shiftClosed = new EventEmitter<any>();

  private readonly shiftApi = inject(ShiftApiService);
  private readonly toast = inject(ToastService);

  loading = signal(false);
  floatAmount: number = 0;
  actualCash: number = 0;
  zResult: any = null;

  submitOpen() {
    this.loading.set(true);
    this.shiftApi.openShift(this.floatAmount).subscribe({
      next: (res: { message: string; shiftId: number }) => {
        this.toast.success(res.message);
        this.loading.set(false);
        this.shiftOpened.emit(res.shiftId);
      },
      error: (err: any) => {
        const msg = err.error?.message || 'Failed to open register';
        console.error('OpenShift error:', err);
        this.toast.error(msg);
        this.loading.set(false);
      }
    });
  }

  submitClose() {
    if (!this.currentShift) return;
    this.loading.set(true);
    const shiftId = this.currentShift.id;
    this.shiftApi.closeShift(shiftId, this.actualCash).subscribe({
      next: (res: any) => {
        this.zResult = res;
        this.mode = 'Result';
        this.loading.set(false);
        this.shiftClosed.emit(res);
      },
      error: () => {
        this.toast.error('Failed to close register');
        this.loading.set(false);
      }
    });
  }
}
