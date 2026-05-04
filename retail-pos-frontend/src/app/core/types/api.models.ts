export type UserRole = 'Admin' | 'Cashier' | 'User';

export interface SessionUser {
  email: string;
  role: UserRole;
  userId: number;
  token: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  role: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  roleId?: number;
  storeId: number;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface Product {
  id: number;
  name: string;
  barcode: string;
  price: number;
  categoryName: string;
  quantity: number;
  taxPercentage: number;
}

export interface Category {
  id: number;
  name: string;
}

export interface TaxConfig {
  id: number;
  name: string;
  taxPercentage: number;
}

export interface CreateProductRequest {
  name: string;
  barcode: string;
  price: number;
  categoryName: string;
  taxPercentage: number;
  initialQuantity: number;
}

export interface CreateCategoryRequest {
  name: string;
}

export interface UpdateCategoryRequest {
  id: number;
  name: string;
}

export interface CreateTaxRequest {
  name: string;
  taxPercentage: number;
}

export interface UpdateTaxRequest {
  id: number;
  name: string;
  taxPercentage: number;
}

export interface CartItem {
  productId: number;
  quantity: number;
  price: number;
  /** Resolved client-side by merging with Products list */
  productName?: string;
  categoryName?: string;
}

export interface Cart {
  cartId: number;
  userId: number;
  items: CartItem[];
  subtotalAmount?: number;
  taxAmount?: number;
  totalAmount: number;
}

export interface AddToCartRequest {
  userId: number;
  productId: number;
  quantity: number;
}

export interface UpdateCartRequest extends AddToCartRequest {}

export interface CheckoutRequest {
  paymentMethod: string;
  couponCode?: string;
  upiId?: string;
  cardNumber?: string;
}

export interface BillLineRequest {
  productId: number;
  quantity: number;
}

export interface CreateBillRequest {
  userId: number;
  items: BillLineRequest[];
  paymentMethod: string;
}

export interface BillSummary {
  billId: number;
  totalAmount: number;
  createdAt: string;
  paymentMethod?: string;
}

export interface InvoiceItem {
  productId: number;
  productName?: string;
  barcode?: string;
  quantity: number;
  price: number;
  taxAmount: number;
  lineTotal?: number;
}

export interface Invoice {
  billId: number;
  invoiceNumber?: string;
  userId: number;
  storeName?: string;
  storeAddress?: string;
  currencyCode?: string;
  cashierName?: string;
  totalAmount: number;
  subtotalAmount?: number;
  taxAmount?: number;
  createdAt: string;
  paymentMethod: string | null;
  paymentStatus: string | null;
  items: InvoiceItem[] | null;
  discountAmount?: number;
  couponCode?: string;
}

export interface DailySales {
  date: string;
  totalRevenue: number;
  totalOrders: number;
}

export interface TopProduct {
  productId: number;
  productName?: string;
  totalQuantitySold: number;
}

export interface CouponRequest {
  code: string;
  discountPercentage: number;
  expiryDate: string;
}

export interface ApplyCouponRequest {
  billId: number;
  code: string;
}

export interface NotificationItem {
  id: number;
  userId?: number;
  message: string | null;
  type: string | null;
  isRead: boolean;
  createdAt: string;
}

export interface Dashboard {
  totalRevenue: number;
  totalOrders: number;
  topProducts: TopProduct[];
  lowStockProducts: Product[];
}

export type PaymentMethod = 'Cash' | 'UPI' | 'Card';

export interface CheckoutSummary {
  billId: number;
  totalAmount: number;
  paymentMethod: PaymentMethod;
  createdAt: string;
}
