# RetailPOS - Enterprise Point of Sale System

RetailPOS is a modern, microservices-based Point of Sale (POS) application designed for high-traffic retail environments. It features a robust .NET backend communicating via event-driven architecture and a sleek, standalone Angular 17 frontend with a responsive, dark-mode native UI.

## 🚀 Key Features

*   **Role-Based Access Control (RBAC):** Distinct interfaces and capabilities for `Admin` vs `Cashier` roles. Cashiers are restricted to cart and shift operations, while Admins have access to the Analytics Dashboard, Direct Billing, and Coupon Generation.
*   **Real-time Cart & Checkout Flow:** A seamless 4-step checkout process (Cart -> Payment -> Invoice) with support for Cash, UPI, and Card. Includes a built-in Cash Calculator to compute "Change to Return" instantly.
*   **Shift Management:** Enforces strict shift states. Cashiers cannot process sales without opening a shift with an initial float, and must close the shift (Z-Report) for daily reconciliation.
*   **Analytics Dashboard:** A beautiful, real-time dashboard powered by Chart.js displaying Top Selling Products, Low Stock Alerts, and Revenue breakdowns.
*   **Hardware Ready:** Features global Barcode Scanner listeners for instant product addition to the cart and a dedicated, printer-friendly thermal receipt view.

## 💡 The "Direct Billing" Feature

In addition to the standard cart flow, RetailPOS includes a **Direct Billing Composer** (available in the Orders/Billing tab for Admins). 

**Benefits of Direct Billing (Why we need it):**
1.  **B2B / Bulk Orders:** When dealing with wholesale or B2B clients, ringing up hundreds of items via barcode is inefficient. Direct Billing allows admins to manually compose a bill by selecting products, entering large quantities, and applying custom overrides instantly.
2.  **System Fallback:** If the frontend cart state or barcode scanner experiences issues, Direct Billing serves as a manual fallback mechanism to ensure sales can still be processed without interruption.
3.  **Post-Dated / Telephone Orders:** For orders placed over the phone or to be fulfilled later, admins can draft a bill manually and hold it without tying up the primary cashier's active cart.

## 🏗️ Architecture Overview

The system is built on a scalable microservices architecture. For deep-dives, see:
*   [High-Level Design (HLD)](docs/HLD.md)
*   [Low-Level Design (LLD)](docs/LLD.md)

### Tech Stack
*   **Frontend:** Angular 17 (Standalone Components), Chart.js, Vanilla CSS (Custom Design System)
*   **Backend:** .NET 8 / C# Minimal APIs
*   **Database:** Entity Framework Core (In-Memory for development)
*   **Messaging:** RabbitMQ & MassTransit (Event-Driven architecture)
*   **Caching:** Redis (Distributed Caching)

## 🛠️ How to Run Locally

### 1. Prerequisites
*   Node.js (v18+)
*   .NET 8 SDK
*   Docker (for Redis and RabbitMQ)

### 2. Start Infrastructure
Run RabbitMQ and Redis via Docker:
```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
docker run -d --name redis -p 6379:6379 redis
```

### 3. Run Microservices
Open separate terminals for each backend service:
```bash
# Auth Service (Port 5001)
cd AuthService
dotnet run

# Product Service (Port 5002)
cd ProductService
dotnet run

# Billing Service (Port 5003)
cd BillingService
dotnet run
```

### 4. Run Frontend
The frontend uses a `proxy.conf.json` to route API requests to the respective microservices.
```bash
cd retail-pos-frontend
npm install
npm start
```
Navigate to `http://localhost:4200`.

### 5. Default Credentials
*   **Admin:** `admin@pos.com` / `admin123`
*   **Cashier:** `cashier@pos.com` / `cashier123`
