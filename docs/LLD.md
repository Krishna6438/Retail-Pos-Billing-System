# Low-Level Design (LLD) - RetailPOS

## 1. Introduction
This document details the internal workings, database schemas, and critical class structures of the RetailPOS system.

## 2. Database Schema (Entity Framework Core)

### 2.1 AuthService DB
*   **Users Table:** `Id` (PK), `Name`, `Email` (Unique), `PasswordHash`, `RoleId` (FK), `StoreId`
*   **Roles Table:** `Id` (PK), `RoleName` ("Admin", "Cashier")

### 2.2 ProductService DB
*   **Products Table:** `Id` (PK), `Name`, `Barcode` (Unique Index), `Price`, `TaxRate`, `Quantity`, `CategoryId` (FK)
*   **Categories Table:** `Id` (PK), `Name`

### 2.3 BillingService DB
*   **Shifts Table:** `Id` (PK), `UserId`, `StartTime`, `EndTime` (Nullable), `InitialFloat`, `ExpectedCash`, `ActualCash`, `Status` (Open, Closed)
*   **Carts Table:** `Id` (PK), `UserId`, `Status` (Active, Held, Completed), `CreatedAt`
*   **CartItems Table:** `Id` (PK), `CartId` (FK), `ProductId`, `Quantity`
*   **Bills Table:** `Id` (PK), `UserId`, `TotalAmount`, `PaymentMethod`, `PaymentStatus`, `CreatedAt`, `CouponCode`
*   **BillItems Table:** `Id` (PK), `BillId` (FK), `ProductId`, `ProductName`, `Quantity`, `Price`, `TaxAmount`, `LineTotal`
*   **Coupons Table:** `Id` (PK), `Code` (Unique), `DiscountPercentage`, `ExpiryDate`, `IsActive`

## 3. Frontend State Management (Angular)

The Angular frontend utilizes **Signals** for reactive state management, heavily relying on custom Store services to isolate side effects from UI components.

### 3.1 AuthStore
*   **State:** `sessionSignal<SessionUser | null>`
*   **Methods:** `login()`, `register()`, `logout()`
*   **Mechanics:** Parses JWT claims to extract `email`, `role`, and `nameidentifier`. Handles local storage persistence. Normalizes the backend role IDs into readable strings (`Admin`, `Cashier`). It acts as the single source of truth for Role-Based Access Control (RBAC) in the UI.

### 3.2 UI Signals (Cart Page)
The `CartPageComponent` uses derived `computed` signals to calculate totals dynamically without digest cycle performance hits:
```typescript
readonly cartTotal = computed(() => { ... });
readonly discountedTotal = computed(() => {
  const total = this.cartTotal();
  const discount = this.discountPercentage();
  return total - (total * (discount / 100));
});
readonly changeToReturn = computed(() => {
  const received = this.amountReceived();
  return Math.max(0, received - this.discountedTotal());
});
```

## 4. API Gateway / Routing
In the local development environment, the Angular CLI proxies traffic to the backend using `proxy.conf.json`:
*   `/api/auth/*` -> `http://localhost:5001`
*   `/api/products/*` -> `http://localhost:5002`
*   `/api/cart/*` & `/api/billing/*` & `/api/shift/*` -> `http://localhost:5003`

## 5. Critical Code Paths

### 5.1 Shift Validation Middleware
Before a `Cart` can be checked out, the `BillingService` validates that the user (Cashier) has an active, open shift.
1. Frontend calls `POST /api/cart/{id}/checkout`
2. `CartService` retrieves the User ID from the HTTP Context (JWT).
3. `ShiftService.GetCurrentShiftAsync(userId)` is invoked. If it returns null or a closed shift, the checkout aborts with `400 Bad Request`.

### 5.2 Direct Billing Flow
The Direct Billing module allows Admins to bypass the standard cart process.
1. Frontend composes an array of `draftLines` directly in memory.
2. The user submits the form calling `POST /api/billing/direct`.
3. The backend bypasses the `Carts` table entirely, immediately inserting records into `Bills` and `BillItems` based on the payload, applying prices from its localized cache of the product catalog.

### 5.3 Cash Calculator Flow
1. User selects "Cash" as `paymentMethod`.
2. UI reveals the `amountReceived` input.
3. User types `500`. The `amountReceived` signal updates.
4. The `changeToReturn` computed signal automatically evaluates `500 - discountedTotal`.
5. If `amountReceived < discountedTotal`, the UI prevents checkout submission to prevent short-changing.
