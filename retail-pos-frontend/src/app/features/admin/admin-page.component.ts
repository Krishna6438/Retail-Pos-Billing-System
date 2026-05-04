import { CommonModule, CurrencyPipe } from '@angular/common';
import {
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  ViewChild,
  inject,
  signal
} from '@angular/core';
import {
  ArcElement,
  BarElement,
  CategoryScale,
  Chart,
  DoughnutController,
  BarController,
  Legend,
  LinearScale,
  Title,
  Tooltip
} from 'chart.js';
import { finalize } from 'rxjs';
import { AdminApiService } from '../../core/services/admin-api.service';
import { BillingApiService } from '../../core/services/billing-api.service';
import { ShiftApiService, Shift as ShiftRecord } from '../../core/services/shift-api.service';
import { Dashboard, DailySales } from '../../core/types/api.models';
import { ToastService } from '../../shared/services/toast.service';

Chart.register(
  ArcElement,
  BarElement,
  CategoryScale,
  DoughnutController,
  BarController,
  Legend,
  LinearScale,
  Title,
  Tooltip
);

@Component({
  selector: 'app-admin-page',
  standalone: true,
  imports: [CommonModule, CurrencyPipe],
  templateUrl: './admin-page.component.html',
  styleUrl: './admin-page.component.scss'
})
export class AdminPageComponent implements AfterViewInit, OnDestroy {
  @ViewChild('revenueCanvas') revenueCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('productsCanvas') productsCanvas!: ElementRef<HTMLCanvasElement>;

  private readonly adminApi = inject(AdminApiService);
  private readonly billingApi = inject(BillingApiService);
  private readonly shiftApi = inject(ShiftApiService);
  private readonly toast = inject(ToastService);

  readonly dashboard = signal<Dashboard | null>(null);
  readonly todaySales = signal<DailySales | null>(null);
  readonly shifts = signal<ShiftRecord[]>([]);
  readonly busy = signal(false);

  private revenueChart?: Chart;
  private productsChart?: Chart;
  private chartsReady = false;

  ngAfterViewInit(): void {
    this.chartsReady = true;
    this.loadDashboard();
  }

  ngOnDestroy(): void {
    this.revenueChart?.destroy();
    this.productsChart?.destroy();
  }

  loadDashboard(): void {
    this.busy.set(true);

    this.adminApi
      .getDashboard()
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: (data) => {
          this.dashboard.set(data);
          this.billingApi.getTodaySales().subscribe({
            next: (sales) => {
              this.todaySales.set(sales);
              if (this.chartsReady) {
                setTimeout(() => this.initCharts(data, sales), 50);
              }
            },
            error: () => {
              if (this.chartsReady) {
                setTimeout(() => this.initCharts(data, null), 50);
              }
            }
          });
        },
        error: () => this.toast.error('Unable to load admin metrics.')
      });

    this.loadShifts();
  }

  loadShifts(): void {
    this.shiftApi.getAllShifts().subscribe({
      next: (data) => this.shifts.set(data),
      error: () => this.toast.error('Unable to load shift records.')
    });
  }

  private initCharts(data: Dashboard, sales: DailySales | null): void {
    this.revenueChart?.destroy();
    this.productsChart?.destroy();

    // Doughnut — today vs total revenue
    if (this.revenueCanvas?.nativeElement) {
      const todayRev = sales?.totalRevenue ?? 0;
      const otherRev = Math.max(0, data.totalRevenue - todayRev);

      this.revenueChart = new Chart(this.revenueCanvas.nativeElement, {
        type: 'doughnut',
        data: {
          labels: ["Today's Revenue", 'Previous Revenue'],
          datasets: [
            {
              data: [todayRev, otherRev || data.totalRevenue || 1],
              backgroundColor: ['rgba(124,92,245,0.85)', 'rgba(168,85,247,0.25)'],
              borderColor: ['#7c5cf5', '#a855f7'],
              borderWidth: 2,
              hoverOffset: 8
            }
          ]
        },
        options: {
          responsive: true,
          cutout: '68%',
          plugins: {
            legend: {
              position: 'bottom',
              labels: { color: '#9aa3c8', font: { family: 'Inter', weight: 600 }, padding: 16 }
            },
            tooltip: {
              callbacks: {
                label: (ctx) => ` ₹${ctx.parsed.toLocaleString('en-IN')}`
              }
            }
          }
        }
      });
    }

    // Bar — top products
    if (this.productsCanvas?.nativeElement && data.topProducts.length) {
      const labels = data.topProducts.map((p) => p.productName || `#${p.productId}`);
      const values = data.topProducts.map((p) => p.totalQuantitySold);

      this.productsChart = new Chart(this.productsCanvas.nativeElement, {
        type: 'bar',
        data: {
          labels,
          datasets: [
            {
              label: 'Units Sold',
              data: values,
              backgroundColor: values.map(
                (_, i) => `hsla(${260 + i * 12}, 72%, ${55 + i * 4}%, 0.85)`
              ),
              borderRadius: 8,
              borderSkipped: false
            }
          ]
        },
        options: {
          responsive: true,
          indexAxis: 'y',
          plugins: {
            legend: { display: false },
            tooltip: {
              callbacks: {
                label: (ctx) => ` ${ctx.parsed.x} units sold`
              }
            }
          },
          scales: {
            x: {
              grid: { color: 'rgba(124,92,245,0.08)' },
              ticks: { color: '#9aa3c8', font: { family: 'Inter' } }
            },
            y: {
              grid: { display: false },
              ticks: { color: '#9aa3c8', font: { family: 'Inter', weight: 700 } }
            }
          }
        }
      });
    }
  }

  getStockPercent(qty: number): number {
    return Math.min(100, Math.round((qty / 10) * 100));
  }
}
