import { CommonModule, CurrencyPipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { CartApiService } from '../../core/services/cart-api.service';
import { ProductsApiService } from '../../core/services/products-api.service';
import { AuthStore } from '../../core/store/auth.store';
import { Category, Product, TaxConfig } from '../../core/types/api.models';
import { ToastService } from '../../shared/services/toast.service';

@Component({
  selector: 'app-products-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, CurrencyPipe],
  templateUrl: './products-page.component.html',
  styleUrl: './products-page.component.scss'
})
export class ProductsPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly productsApi = inject(ProductsApiService);
  private readonly cartApi = inject(CartApiService);
  private readonly toast = inject(ToastService);

  readonly authStore = inject(AuthStore);
  readonly products = signal<Product[]>([]);
  readonly categories = signal<Category[]>([]);
  readonly taxes = signal<TaxConfig[]>([]);
  readonly lowStock = signal<Product[]>([]);
  readonly busy = signal(false);
  readonly search = signal('');
  readonly barcodeSearch = signal('');
  readonly editingCategoryId = signal<number | null>(null);
  readonly editingTaxId = signal<number | null>(null);
  readonly activeTab = signal<'catalog' | 'manage'>('catalog');

  readonly filteredProducts = computed(() => {
    const query = this.search().trim().toLowerCase();
    if (!query) return this.products();
    return this.products().filter(
      (p) =>
        p.name.toLowerCase().includes(query) ||
        p.barcode.toLowerCase().includes(query) ||
        p.categoryName.toLowerCase().includes(query)
    );
  });

  readonly productForm = this.fb.nonNullable.group({
    name: ['', Validators.required],
    barcode: ['', Validators.required],
    price: [0, [Validators.required, Validators.min(0.01)]],
    categoryName: ['', Validators.required],
    taxPercentage: [0, [Validators.required, Validators.min(0)]],
    initialQuantity: [1, [Validators.required, Validators.min(1)]]
  });

  readonly categoryForm = this.fb.nonNullable.group({ name: ['', Validators.required] });
  readonly taxForm = this.fb.nonNullable.group({
    name: ['', Validators.required],
    taxPercentage: [0, [Validators.required, Validators.min(0)]]
  });

  constructor() { this.loadCatalog(); }

  loadCatalog(): void {
    this.busy.set(true);
    forkJoin({
      products: this.productsApi.getProducts(),
      categories: this.productsApi.getCategories(),
      taxes: this.productsApi.getTaxes(),
      lowStock: this.productsApi.getLowStock()
    }).subscribe({
      next: ({ products, categories, taxes, lowStock }) => {
        this.products.set(products);
        this.categories.set(categories);
        this.taxes.set(taxes);
        this.lowStock.set(lowStock);
        this.busy.set(false);
      },
      error: () => {
        this.toast.error('Unable to load products.');
        this.busy.set(false);
      }
    });
  }

  addToCart(productId: number, quantity: number): void {
    this.cartApi.addItem({ userId: this.authStore.userId(), productId, quantity }).subscribe({
      next: (r) => this.toast.success(r.message),
      error: () => this.toast.error('Unable to add item to cart.')
    });
  }

  addByBarcode(): void {
    const barcode = this.barcodeSearch().trim();

    if (!barcode) {
      this.toast.warning('Enter or scan a barcode first.');
      return;
    }

    this.productsApi.getProductByBarcode(barcode).subscribe({
      next: (product) => {
        this.addToCart(product.id, 1);
        this.barcodeSearch.set('');
      },
      error: () => this.toast.error('No product found for this barcode.')
    });
  }

  generateBarcode(): void {
    const barcode = `8901262${Math.floor(100000 + Math.random() * 900000)}`;
    this.productForm.patchValue({ barcode });
  }

  submitProduct(): void {
    if (this.productForm.invalid) { this.productForm.markAllAsTouched(); return; }
    this.productsApi.createProduct(this.productForm.getRawValue()).subscribe({
      next: () => {
        this.toast.success('Product created successfully.');
        this.productForm.reset({ name: '', barcode: '', price: 0, categoryName: '', taxPercentage: 0, initialQuantity: 1 });
        this.loadCatalog();
      },
      error: () => this.toast.error('Unable to create product.')
    });
  }

  submitCategory(): void {
    if (this.categoryForm.invalid) { this.categoryForm.markAllAsTouched(); return; }
    const payload = this.categoryForm.getRawValue();
    const request = this.editingCategoryId()
      ? this.productsApi.updateCategory({ id: this.editingCategoryId()!, ...payload })
      : this.productsApi.createCategory(payload);
    request.subscribe({
      next: (r) => {
        this.toast.success(r.message);
        this.categoryForm.reset({ name: '' });
        this.editingCategoryId.set(null);
        this.loadCatalog();
      },
      error: () => this.toast.error('Unable to save category.')
    });
  }

  editCategory(category: Category): void {
    this.editingCategoryId.set(category.id);
    this.categoryForm.patchValue({ name: category.name });
  }

  deleteCategory(categoryId: number): void {
    this.productsApi.deleteCategory(categoryId).subscribe({
      next: (r) => { this.toast.info(r.message); this.loadCatalog(); },
      error: () => this.toast.error('Unable to delete category.')
    });
  }

  submitTax(): void {
    if (this.taxForm.invalid) { this.taxForm.markAllAsTouched(); return; }
    const payload = this.taxForm.getRawValue();
    const request = this.editingTaxId()
      ? this.productsApi.updateTax({ id: this.editingTaxId()!, ...payload })
      : this.productsApi.createTax(payload);
    request.subscribe({
      next: (r) => {
        this.toast.success(r.message);
        this.taxForm.reset({ name: '', taxPercentage: 0 });
        this.editingTaxId.set(null);
        this.loadCatalog();
      },
      error: () => this.toast.error('Unable to save tax setup.')
    });
  }

  editTax(tax: TaxConfig): void {
    this.editingTaxId.set(tax.id);
    this.taxForm.patchValue({ name: tax.name, taxPercentage: tax.taxPercentage });
  }

  deleteTax(taxId: number): void {
    this.productsApi.deleteTax(taxId).subscribe({
      next: (r) => { this.toast.info(r.message); this.loadCatalog(); },
      error: () => this.toast.error('Unable to delete tax config.')
    });
  }

  deleteProduct(productId: number): void {
    this.productsApi.deleteProduct(productId).subscribe({
      next: (r) => { this.toast.info(r.message); this.loadCatalog(); },
      error: () => this.toast.error('Unable to delete product.')
    });
  }

  getCategoryIcon(categoryName: string): string {
    const cat = (categoryName || '').toLowerCase();
    if (cat.includes('food') || cat.includes('snack') || cat.includes('bev')) return '🍱';
    if (cat.includes('electronic') || cat.includes('tech')) return '💻';
    if (cat.includes('cloth') || cat.includes('wear')) return '👕';
    if (cat.includes('health') || cat.includes('pharma')) return '💊';
    if (cat.includes('beauty') || cat.includes('cosmetic')) return '💄';
    if (cat.includes('book') || cat.includes('stationery')) return '📚';
    if (cat.includes('grocery')) return '🛒';
    return '📦';
  }
}
