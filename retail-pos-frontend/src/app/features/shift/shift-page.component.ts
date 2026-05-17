import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ShiftApiService, Shift } from '../../core/services/shift-api.service';
import { ToastService } from '../../shared/services/toast.service';
import { Router } from '@angular/router';
import { AuthStore } from '../../core/store/auth.store';


@Component({
  selector: 'app-shift-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-shell">
      <section class="section-card hero-panel animate-fade-up">
        <div>
          <p class="label-chip">Shift Management</p>
          <h1 class="section-title">Control your cash drawer</h1>
          <p class="section-subtitle">Open your register at the start of your shift and close it when you're done.</p>
        </div>
      </section>

      @if (loading()) {
        <div style="display:flex;justify-content:center;padding:3rem">
          <div class="spinner"></div>
        </div>
      } @else {
        <div class="shift-container">
          @if (!currentShift()) {
            <!-- Open Shift Form -->
            <section class="section-card animate-fade-up" style="max-width: 500px; margin: 0 auto;">
              <h2 class="section-title">Open Register</h2>
              <p class="section-subtitle">Enter the starting cash float in your drawer to begin processing transactions.</p>
              
              <div class="field" style="margin-top: 1.5rem">
                <label style="display:block; font-weight:600; margin-bottom:0.5rem">Starting Float (₹)</label>
                <input type="number" [(ngModel)]="floatAmount" min="0" step="100" 
                  style="padding:1rem; border-radius:var(--radius-sm); border:var(--border-medium); width:100%; font-size:1.25rem; font-family:monospace" />
              </div>
              
              <button class="btn btn-primary" style="margin-top: 1.5rem; width: 100%; padding: 1rem;" (click)="submitOpen()" [disabled]="processing()">
                {{ processing() ? 'Opening...' : '🚀 Open Register & Start Shift' }}
              </button>
            </section>
          } @else if (currentShift() && !zResult()) {
            <!-- Active Shift Dashboard -->
            <div class="dashboard-grid">
              <section class="section-card animate-fade-up">
                <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:1.5rem">
                  <div>
                    <h2 class="section-title" style="margin:0">Active Shift</h2>
                    <p class="section-subtitle" style="margin:0">Started at {{ currentShift()?.openedAt | date:'medium' }}</p>
                  </div>
                  <span class="label-chip" style="background:var(--success-soft); color:var(--success)">● Live</span>
                </div>

                <div class="stats-grid">
                  <div class="stat-card">
                    <span class="stat-label">Starting Float</span>
                    <strong class="stat-value">₹{{ currentShift()?.startingFloat | number:'1.2-2' }}</strong>
                  </div>
                  <div class="stat-card">
                    <span class="stat-label">Expected Cash</span>
                    <strong class="stat-value" style="color:var(--brand-1)">₹{{ currentShift()?.expectedCash | number:'1.2-2' }}</strong>
                  </div>
                </div>

                <div style="margin-top: 2rem">
                  <h3 style="font-size:0.9rem; color:var(--ink-2); text-transform:uppercase; letter-spacing:1px; margin-bottom:1rem">Shift Reconciliation</h3>
                  <div class="field">
                    <label style="display:block; font-weight:600; margin-bottom:0.5rem">Actual Cash in Drawer (₹)</label>
                    <input type="number" [(ngModel)]="actualCash" min="0" step="10" placeholder="Enter counted amount"
                      style="padding:1rem; border-radius:var(--radius-sm); border:var(--border-medium); width:100%; font-size:1.25rem; font-family:monospace" />
                  </div>
                  
                  <button class="btn btn-primary" style="margin-top: 1.5rem; width: 100%; padding:1rem" (click)="submitClose()" [disabled]="processing()">
                    {{ processing() ? 'Closing...' : '🔒 Close Register & Sign Out' }}
                  </button>
                </div>
              </section>

              <section class="section-card animate-fade-up" style="animation-delay: 0.1s">
                <h2 class="section-title">Cash Information</h2>
                <p class="section-subtitle">Real-time breakdown of your drawer contents.</p>

                <div class="info-list">
                  <div class="info-item">
                    <span>Opening Balance</span>
                    <strong>₹{{ currentShift()?.startingFloat | number:'1.2-2' }}</strong>
                  </div>
                  <div class="info-item">
                    <span>Net Cash Sales</span>
                    <strong>₹{{ (currentShift()?.expectedCash ?? 0) - (currentShift()?.startingFloat ?? 0) | number:'1.2-2' }}</strong>
                  </div>
                  <div class="info-item" style="border-top: 1px dashed var(--border-medium); margin-top: 1rem; padding-top: 1rem;">
                    <span>Expected Final Cash</span>
                    <strong style="font-size: 1.25rem">₹{{ currentShift()?.expectedCash | number:'1.2-2' }}</strong>
                  </div>
                </div>

                <div class="alert alert-info" style="margin-top: 2rem; background: var(--surface-3); padding: 1rem; border-radius: var(--radius-sm); font-size: 0.85rem; color: var(--ink-2)">
                  💡 <strong>Tip:</strong> Ensure you count all coins and notes physically before entering the amount. Any discrepancy will be logged as a variance.
                </div>
              </section>
            </div>
          } @else if (zResult()) {
            <!-- Result Panel -->
            <section class="section-card animate-fade-up" style="max-width: 600px; margin: 0 auto; text-align: center;">
              <div class="success-check" style="width:64px; height:64px; font-size:32px; margin: 0 auto 1.5rem">✓</div>
              <h2 class="section-title">Shift Closed Successfully</h2>
              <p class="section-subtitle">The reconciliation report has been generated and saved.</p>
              
              <div class="z-report-card">
                <div class="z-row"><span>Opening Float:</span> <strong>₹{{ zResult()?.startingFloat | number:'1.2-2' }}</strong></div>
                <div class="z-row"><span>System Expected:</span> <strong>₹{{ zResult()?.expectedCash | number:'1.2-2' }}</strong></div>
                <div class="z-row"><span>Actual Counted:</span> <strong>₹{{ zResult()?.actualCash | number:'1.2-2' }}</strong></div>
                <hr />
                <div class="z-row variance" [style.color]="(zResult()?.variance ?? 0) >= 0 ? 'var(--success)' : 'var(--danger)'">
                  <span>Variance (Over/Short):</span> 
                  <strong>₹{{ zResult()?.variance | number:'1.2-2' }}</strong>
                </div>
              </div>
              
              <button class="btn btn-primary" style="margin-top: 2rem; width: 100%; padding: 1rem" (click)="acknowledge()">
                Return to Overview
              </button>
            </section>
          }
        </div>
      }
    </div>

    <!-- ─── Shift Opened Success Modal ─── -->
    @if (showStartModal()) {
      <div class="modal-overlay" (click)="dismissModal()">
        <div class="modal-card animate-fade-up" (click)="$event.stopPropagation()">
          <div class="modal-icon">🚀</div>
          <h2 class="modal-title">Register is Open!</h2>
          <p class="modal-subtitle">
            Your shift has started successfully.<br>
            You're all set to process transactions.
          </p>
          <div class="modal-meta">
            <div class="modal-meta-row">
              <span>Starting Float</span>
              <strong>₹{{ currentShift()?.startingFloat | number:'1.2-2' }}</strong>
            </div>
            <div class="modal-meta-row">
              <span>Shift Started</span>
              <strong>{{ currentShift()?.openedAt | date:'h:mm a':'+0530' }}</strong>
            </div>
          </div>
          <button class="btn btn-primary modal-cta" (click)="dismissModal()">
            Start Selling &nbsp;→
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    .page-shell { padding: 2rem; max-width: 1200px; margin: 0 auto; }
    .shift-container { margin-top: 2rem; }
    .dashboard-grid { display: grid; grid-template-columns: 1.2fr 0.8fr; gap: 2rem; }
    
    .stats-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    .stat-card { background: var(--surface-2); padding: 1.5rem; border-radius: var(--radius-md); border: var(--border-soft); }
    .stat-label { display: block; font-size: 0.75rem; color: var(--ink-2); text-transform: uppercase; letter-spacing: 1px; margin-bottom: 0.5rem; }
    .stat-value { font-size: 1.75rem; font-family: monospace; }

    .info-list { margin-top: 1.5rem; }
    .info-item { display: flex; justify-content: space-between; padding: 0.75rem 0; color: var(--ink-1); }
    .info-item span { color: var(--ink-2); }

    .z-report-card { background: var(--surface-3); padding: 2rem; border-radius: var(--radius-md); margin-top: 2rem; font-family: monospace; text-align: left; }
    .z-row { display: flex; justify-content: space-between; margin-bottom: 1rem; font-size: 1.1rem; }
    .z-row hr { border: none; border-top: 1px dashed var(--border-medium); margin: 1rem 0; }
    .variance { font-weight: bold; border-top: 1px solid var(--border-medium); padding-top: 1rem; }

    /* ─── Success Modal ─── */
    .modal-overlay {
      position: fixed; inset: 0;
      background: rgba(0,0,0,0.65);
      backdrop-filter: blur(6px);
      display: flex; align-items: center; justify-content: center;
      z-index: 1000;
    }
    .modal-card {
      background: var(--surface-2);
      border: 1px solid var(--border-soft);
      border-radius: var(--radius-lg);
      padding: 2.5rem 2rem;
      max-width: 400px;
      width: 90%;
      text-align: center;
      box-shadow: 0 24px 64px rgba(0,0,0,0.5);
    }
    .modal-icon { font-size: 3rem; margin-bottom: 1rem; }
    .modal-title { font-size: 1.5rem; font-weight: 700; color: var(--ink-1); margin: 0 0 0.5rem; }
    .modal-subtitle { color: var(--ink-2); font-size: 0.95rem; line-height: 1.6; margin: 0 0 1.5rem; }
    .modal-meta {
      background: var(--surface-3);
      border-radius: var(--radius-md);
      padding: 1rem 1.25rem;
      margin-bottom: 1.5rem;
    }
    .modal-meta-row {
      display: flex; justify-content: space-between;
      padding: 0.4rem 0;
      font-size: 0.9rem;
      color: var(--ink-2);
    }
    .modal-meta-row strong { color: var(--ink-1); }
    .modal-cta { width: 100%; padding: 0.9rem; font-size: 1rem; font-weight: 600; letter-spacing: 0.5px; }

    @media (max-width: 900px) {
      .dashboard-grid { grid-template-columns: 1fr; }
    }
  `]
})
export class ShiftPageComponent implements OnInit {
  private readonly shiftApi = inject(ShiftApiService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  readonly authStore = inject(AuthStore);

  loading = signal(true);
  processing = signal(false);
  showStartModal = signal(false);

  currentShift = signal<Shift | null>(null);
  zResult = signal<any>(null);

  floatAmount: number = 0;
  actualCash: number = 0;

  ngOnInit() {
    this.loadShift();
  }

  loadShift() {
    this.loading.set(true);
    this.shiftApi.getCurrentShift().subscribe({
      next: (shift) => {
        if (shift && shift.id) {
          this.currentShift.set(shift);
          // Set actualCash to 0 by default as per user request "no sample data"
          this.actualCash = 0;
        } else {
          this.currentShift.set(null);
        }
        this.loading.set(false);
      },
      error: () => {
        this.currentShift.set(null);
        this.loading.set(false);
      }
    });
  }

  submitOpen() {
    this.processing.set(true);
    this.shiftApi.openShift(this.floatAmount).subscribe({
      next: (res: any) => {
        this.toast.success('Shift opened successfully.');
        this.processing.set(false);
        this.loadShift();
        this.showStartModal.set(true);
      },
      error: (err: any) => {
        const msg = err.error?.message || 'Failed to open register';
        this.toast.error(msg);
        this.processing.set(false);
      }
    });
  }

  submitClose() {
    const shift = this.currentShift();
    if (!shift) return;

    this.processing.set(true);
    this.shiftApi.closeShift(shift.id, this.actualCash).subscribe({
      next: (res: any) => {
        this.zResult.set(res);
        this.currentShift.set(null);
        this.processing.set(false);
        this.toast.success('Shift closed. Reconciling...');
      },
      error: (err: any) => {
        const msg = err.error?.message || 'Failed to close register';
        this.toast.error(msg);
        this.processing.set(false);
      }
    });
  }

  dismissModal() {
    this.showStartModal.set(false);
    this.router.navigateByUrl('/cart');
  }

  acknowledge() {
    this.zResult.set(null);
    this.authStore.clear();
    this.router.navigateByUrl('/auth');
  }
}
