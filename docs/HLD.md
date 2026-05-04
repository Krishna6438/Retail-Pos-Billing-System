# High-Level Design (HLD) - RetailPOS

## 1. System Overview
RetailPOS is an enterprise-grade Point of Sale system tailored for high-volume retail environments. The system adopts a **Microservices Architecture** to ensure modularity, independent scalability, and fault isolation.

The platform is designed to handle core retail workflows:
- User Authentication & Authorization (RBAC)
- Product & Inventory Management
- Real-time Cart Management & Checkout
- Shift Reconciliation & Auditing
- Analytics & Reporting

## 2. Architectural Pattern
The system leverages an **Event-Driven Architecture (EDA)** combined with traditional RESTful APIs. 
- **Synchronous Communication:** Client (Angular) to Microservices via REST API using HTTP/HTTPS.
- **Asynchronous Communication:** Microservice to Microservice via **RabbitMQ** (Message Broker) and **MassTransit** (Service Bus).

## 3. System Components

### 3.1 Frontend App (Client)
- **Tech:** Angular 17, Standalone Components, Vanilla CSS
- **Responsibility:** Provides the UI for Cashiers (sales, carts, shifts) and Admins (analytics, direct billing, product management).
- **Communication:** Communicates with backend microservices via an API Gateway or directly through a reverse proxy (configured via `proxy.conf.json` in local development).

### 3.2 Microservices
The backend is split into three highly cohesive, loosely coupled microservices built on **.NET 8**.

#### A. AuthService
- **Responsibility:** Manages users, credentials, JWT generation, and Role-Based Access Control (Admin vs. Cashier).
- **Database:** Owns the `Users` and `Roles` tables.
- **Interactions:** Issues JWTs which are validated statelessly by other services.

#### B. ProductService
- **Responsibility:** Manages the product catalog, categories, pricing, and inventory levels. Handles barcode lookups.
- **Database:** Owns the `Products` and `Categories` tables.
- **Interactions:** Publishes `ProductCreated` or `StockUpdated` events to RabbitMQ.

#### C. BillingService
- **Responsibility:** The core operational engine. Manages Carts, Checkout, Invoice generation, Shift Management, and Coupons.
- **Database:** Owns `Carts`, `Bills` (Invoices), `BillItems`, `Shifts`, and `Coupons`.
- **Interactions:** Subscribes to Product events (to cache product names/prices). Publishes `OrderCompleted` events.

### 3.3 Infrastructure & Middleware

#### A. RabbitMQ / MassTransit (Message Broker)
- Acts as the central nervous system for inter-service communication.
- Decouples the `BillingService` from the `ProductService`. For example, when a sale is finalized, `BillingService` publishes a `StockDeductionRequest`, which `ProductService` consumes to update inventory asynchronously.

#### B. Redis (Distributed Cache)
- Used by `BillingService` (and others) to cache frequently accessed, read-heavy data.
- **Use Case:** Caching Cart states to survive service restarts and caching the Product Catalog to reduce latency during barcode scanning.

## 4. High-Level Data Flow (Checkout Process)
1. **Add to Cart:** The Frontend sends a Barcode to `BillingService`. `BillingService` checks its Redis Cache for product details. If not found, it queries `ProductService`.
2. **Checkout:** Cashier initiates payment. `BillingService` validates the active Shift, verifies the Coupon, and creates a `Bill` record in the database.
3. **Event Emitted:** `BillingService` publishes an `OrderCompletedEvent` to RabbitMQ.
4. **Inventory Deduction:** `ProductService` consumes the event and decrements the `StockQuantity` for the purchased items.
5. **Response:** Frontend receives the Invoice ID, fetches the PDF/Receipt view, and triggers the print dialog.

## 5. Resilience and Fault Tolerance
- **Polly (Circuit Breaker & Retries):** Integrated into internal HTTP calls (e.g., if `BillingService` must synchronously call `ProductService`).
- **Idempotency:** MassTransit consumers are designed to be idempotent to handle message retries without deducting stock twice.
