from docx import Document
from docx.shared import Pt, RGBColor, Inches, Cm
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_ALIGN_VERTICAL
from docx.oxml.ns import qn
from docx.oxml import OxmlElement
import copy

doc = Document()

# ─── Page Margins ───────────────────────────────────────────────────────────
section = doc.sections[0]
section.top_margin    = Cm(2)
section.bottom_margin = Cm(2)
section.left_margin   = Cm(2.5)
section.right_margin  = Cm(2.5)

# ─── Helpers ────────────────────────────────────────────────────────────────
def set_heading(para, text, level=1, color=(0, 71, 171)):
    para.clear()
    run = para.add_run(text)
    run.bold = True
    run.font.color.rgb = RGBColor(*color)
    run.font.size = Pt(18 if level == 1 else 14 if level == 2 else 12)
    para.paragraph_format.space_before = Pt(18)
    para.paragraph_format.space_after  = Pt(6)

def add_heading(doc, text, level=1, color=(0, 71, 171)):
    p = doc.add_paragraph()
    set_heading(p, text, level, color)
    return p

def add_body(doc, text):
    p = doc.add_paragraph(text)
    p.paragraph_format.space_after = Pt(4)
    for run in p.runs:
        run.font.size = Pt(11)
    return p

def add_bullet(doc, text):
    p = doc.add_paragraph(style='List Bullet')
    p.paragraph_format.space_after = Pt(3)
    run = p.add_run(text)
    run.font.size = Pt(11)
    return p

def add_sub_bullet(doc, label, detail):
    p = doc.add_paragraph(style='List Bullet 2')
    p.paragraph_format.space_after = Pt(2)
    r1 = p.add_run(label + ": ")
    r1.bold = True
    r1.font.size = Pt(11)
    r2 = p.add_run(detail)
    r2.font.size = Pt(11)
    return p

def shade_cell(cell, fill_hex):
    tc = cell._tc
    tcPr = tc.get_or_add_tcPr()
    shd = OxmlElement('w:shd')
    shd.set(qn('w:val'), 'clear')
    shd.set(qn('w:color'), 'auto')
    shd.set(qn('w:fill'), fill_hex)
    tcPr.append(shd)

def make_table(doc, headers, rows, col_widths=None):
    table = doc.add_table(rows=1 + len(rows), cols=len(headers))
    table.style = 'Table Grid'
    table.alignment = WD_TABLE_ALIGNMENT.CENTER

    # header row
    hdr = table.rows[0]
    for i, h in enumerate(headers):
        cell = hdr.cells[i]
        shade_cell(cell, '003087')
        p = cell.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        run = p.add_run(h)
        run.bold = True
        run.font.size = Pt(10)
        run.font.color.rgb = RGBColor(255, 255, 255)
        cell.vertical_alignment = WD_ALIGN_VERTICAL.CENTER

    # data rows
    for ri, row_data in enumerate(rows):
        row = table.rows[ri + 1]
        fill = 'EBF0FA' if ri % 2 == 0 else 'FFFFFF'
        for ci, val in enumerate(row_data):
            cell = row.cells[ci]
            shade_cell(cell, fill)
            p = cell.paragraphs[0]
            p.alignment = WD_ALIGN_PARAGRAPH.LEFT
            run = p.add_run(val)
            run.font.size = Pt(10)
            cell.vertical_alignment = WD_ALIGN_VERTICAL.CENTER

    if col_widths:
        for row in table.rows:
            for i, w in enumerate(col_widths):
                row.cells[i].width = Cm(w)
    return table

def add_divider(doc):
    p = doc.add_paragraph()
    pPr = p._p.get_or_add_pPr()
    pBdr = OxmlElement('w:pBdr')
    bottom = OxmlElement('w:bottom')
    bottom.set(qn('w:val'), 'single')
    bottom.set(qn('w:sz'), '6')
    bottom.set(qn('w:space'), '1')
    bottom.set(qn('w:color'), '0047AB')
    pBdr.append(bottom)
    pPr.append(pBdr)
    p.paragraph_format.space_before = Pt(4)
    p.paragraph_format.space_after  = Pt(8)

def add_diagram_note(doc, title, lines):
    """Renders a diagram as a styled bordered table."""
    table = doc.add_table(rows=1, cols=1)
    table.style = 'Table Grid'
    cell = table.cell(0, 0)
    shade_cell(cell, 'F0F4FF')
    p = cell.paragraphs[0]
    run = p.add_run(title + "\n")
    run.bold = True
    run.font.color.rgb = RGBColor(0, 71, 171)
    run.font.size = Pt(11)
    for line in lines:
        r = p.add_run(line + "\n")
        r.font.name = 'Courier New'
        r.font.size = Pt(9)
        r.font.color.rgb = RGBColor(30, 30, 80)
    doc.add_paragraph()


# ════════════════════════════════════════════════════════════════════════════
# TITLE PAGE
# ════════════════════════════════════════════════════════════════════════════
title_para = doc.add_paragraph()
title_para.alignment = WD_ALIGN_PARAGRAPH.CENTER
tr = title_para.add_run("\n\n\nRetailPOS Enterprise")
tr.bold = True
tr.font.size = Pt(32)
tr.font.color.rgb = RGBColor(0, 71, 171)

sub = doc.add_paragraph()
sub.alignment = WD_ALIGN_PARAGRAPH.CENTER
sr = sub.add_run("A Modern Microservices Point-of-Sale System")
sr.font.size = Pt(16)
sr.font.color.rgb = RGBColor(80, 80, 100)

doc.add_paragraph()
tech = doc.add_paragraph()
tech.alignment = WD_ALIGN_PARAGRAPH.CENTER
tr2 = tech.add_run(".NET 8  •  Angular 17  •  RabbitMQ  •  Redis  •  EF Core")
tr2.font.size = Pt(12)
tr2.italic = True
tr2.font.color.rgb = RGBColor(100, 100, 120)

doc.add_paragraph("\n\n\n")
doc.add_page_break()


# ════════════════════════════════════════════════════════════════════════════
# 1. PROJECT OVERVIEW
# ════════════════════════════════════════════════════════════════════════════
add_heading(doc, "1. Project Overview")
add_divider(doc)
add_body(doc,
    "RetailPOS Enterprise is a production-grade, distributed Point of Sale system built for "
    "high-traffic retail environments. It demonstrates real-world patterns including Microservices "
    "Architecture, Event-Driven Communication, and RBAC Security.")

add_heading(doc, "Objectives", level=2)
for obj in [
    "Enable fast, efficient checkout with barcode scanning and multi-payment support.",
    "Enforce daily Shift Management and financial audit trails (Z-Reports).",
    "Provide real-time Admin Analytics with inventory health monitoring.",
    "Ensure role-based access — separating Admin and Cashier workflows.",
    "Build a scalable, loosely coupled backend ready for enterprise expansion."
]:
    add_bullet(doc, obj)

doc.add_page_break()


# ════════════════════════════════════════════════════════════════════════════
# 2. TECHNOLOGY STACK
# ════════════════════════════════════════════════════════════════════════════
add_heading(doc, "2. Technology Stack")
add_divider(doc)

make_table(doc,
    headers=["Layer", "Technology", "Purpose"],
    rows=[
        ["Frontend",      "Angular 17 (Standalone Components)", "UI Layer - Cashier & Admin Dashboards"],
        ["State Mgmt",    "Angular Signals + Computed()",        "Reactive real-time UI updates without RxJS complexity"],
        ["Styling",       "Vanilla CSS (Custom Design System)",   "Premium dark-mode UI with CSS variables"],
        ["Charts",        "Chart.js",                             "Revenue Doughnut & Top Products Bar Chart"],
        ["Backend",       ".NET 8 / C# (Minimal APIs)",           "High-performance REST endpoints"],
        ["ORM",           "Entity Framework Core (Code First)",   "Database modelling and migrations"],
        ["Database",      "SQL Server",                           "Persistent data storage for all services"],
        ["Auth",          "JWT (JSON Web Tokens)",                 "Stateless, secure authentication"],
        ["Messaging",     "RabbitMQ + MassTransit",               "Async event-driven inter-service communication"],
        ["Caching",       "Redis",                                 "Distributed caching for product lookups"],
        ["Security",      "RBAC (Admin / Cashier Roles)",          "Role-gated routes and API endpoints"],
    ],
    col_widths=[3.5, 5.5, 7.5]
)
doc.add_paragraph()
doc.add_page_break()


# ════════════════════════════════════════════════════════════════════════════
# 3. HIGH LEVEL DESIGN (HLD)
# ════════════════════════════════════════════════════════════════════════════
add_heading(doc, "3. High-Level Design (HLD)")
add_divider(doc)
add_body(doc,
    "The system adopts a Microservices Architecture. Each service owns its database "
    "(Database-per-Service pattern) and communicates via REST (synchronous) or RabbitMQ (asynchronous).")

add_heading(doc, "Architecture Diagram", level=2)
add_diagram_note(doc, "RetailPOS High-Level Architecture", [
    "",
    "  +-------------------+       REST / HTTP        +----------------------+",
    "  |  Angular 17 UI    |  ─────────────────────>  |   API Gateway/Proxy  |",
    "  |  (Frontend Client)|                           +----------+-----------+",
    "  +-------------------+                                      |",
    "                                          ┌──────────────────┼──────────────────┐",
    "                                          ▼                  ▼                  ▼",
    "                               +----------+------+  +--------+------+  +--------+-------+",
    "                               |  Auth Service   |  | Product Svc   |  | Billing Svc    |",
    "                               |  JWT + Roles    |  | Catalog + Inv.|  | Cart+Shifts+   |",
    "                               +--------+--------+  +-------+-------+  | Invoices       |",
    "                                        |                   |           +--------+-------+",
    "                               +--------+-------------------+-----------+        |",
    "                               |         RabbitMQ Message Broker                |",
    "                               |  (OrderCompletedEvent / StockUpdatedEvent)      |",
    "                               +-----------+-------------------+----------------+",
    "                                           |                   |",
    "                               +-----------+----+   +----------+----------+",
    "                               | Notification   |   | Admin Service (BFF) |",
    "                               | Service        |   | Aggregates Metrics  |",
    "                               +----------------+   +---------------------+",
    "                                                             |",
    "                                                     +-------+-------+",
    "                                                     |  Redis Cache  |",
    "                                                     +---------------+",
])

add_heading(doc, "Microservices Breakdown", level=2)
make_table(doc,
    headers=["Service", "Port", "Responsibility"],
    rows=[
        ["AuthService",         "5001", "User login, registration, JWT token issuance, role management"],
        ["ProductService",      "5002", "Product catalog, categories, pricing, inventory, barcode lookup"],
        ["BillingService",      "5003", "Cart, Checkout, Shift Management, Coupons, Invoice generation"],
        ["NotificationService", "5004", "Consumes events from RabbitMQ, stores user notifications"],
        ["AdminService",        "5005", "Aggregates data from all services for the Admin Dashboard (BFF pattern)"],
    ],
    col_widths=[4, 2.5, 10]
)
doc.add_paragraph()
doc.add_page_break()


# ════════════════════════════════════════════════════════════════════════════
# 4. LOW LEVEL DESIGN (LLD)
# ════════════════════════════════════════════════════════════════════════════
add_heading(doc, "4. Low-Level Design (LLD)")
add_divider(doc)
add_body(doc,
    "The LLD describes the internal patterns, database schema, and service interactions at a code level.")

add_heading(doc, "4.1 Service Internal Pattern (Controller → Service → Repository)", level=2)
add_diagram_note(doc, "BillingService Internal Flow - POST /checkout", [
    "",
    "  Angular UI",
    "      │  POST /billing/checkout/{cartId}",
    "      ▼",
    "  BillingController.cs",
    "      │  Extracts User ID from JWT (ClaimTypes.NameIdentifier)",
    "      │  Validates cart ownership",
    "      ▼",
    "  BillingService.cs",
    "      │  CheckoutFromCart(cartId, token)",
    "      │  ├─ Calls ProductClient → GET /products/{id}  (HTTP call to ProductService)",
    "      │  ├─ Validates Shift is Open (ShiftService)",
    "      │  ├─ Calculates tax + totals",
    "      │  └─ Saves Bill + BillItems to DB via BillingRepository",
    "      ▼",
    "  BillingRepository.cs",
    "      │  _context.Bills.Add(bill)  → EF Core → SQL Server",
    "      ▼",
    "  RabbitMQPublisher.cs",
    "      │  PublishBillCreated(OrderCompletedEvent)",
    "      ▼",
    "  Response: BillSummaryDTO → Frontend",
])

add_heading(doc, "4.2 Frontend Angular Signals Architecture", level=2)
add_body(doc,
    "Angular 17 Signals are used for all real-time state in the Cart Page. "
    "This replaces older BehaviorSubject/Observable patterns for UI performance.")

add_diagram_note(doc, "CartPageComponent Signal Flow", [
    "",
    "  amountReceived = signal<number>(0)          ← User types cash amount",
    "  discountedTotal = computed(() => {           ← Auto-calculated",
    "      const total = cartTotal();",
    "      return total - (total * discount / 100);",
    "  });",
    "  changeToReturn = computed(() => {            ← Auto-calculated",
    "      return Math.max(0, amountReceived() - discountedTotal());",
    "  });",
    "",
    "  UI Template  →  {{ changeToReturn() }}       ← Instantly reactive, no click needed",
])

doc.add_page_break()


# ════════════════════════════════════════════════════════════════════════════
# 5. ER DIAGRAM
# ════════════════════════════════════════════════════════════════════════════
add_heading(doc, "5. Entity-Relationship (ER) Diagram")
add_divider(doc)

add_diagram_note(doc, "Database Entity Relationships", [
    "",
    "  [USERS]  ──────1──────────< [SHIFTS]",
    "     │  id (PK)                  id (PK)",
    "     │  email                    user_id (FK)",
    "     │  role                     starting_float",
    "     │                           expected_cash",
    "     │                           actual_cash",
    "     │                           variance",
    "     │                           status (Open/Closed)",
    "     │",
    "     └────────1──────────< [BILLS]",
    "                               id (PK)",
    "                               user_id (FK)",
    "                               shift_id (FK)",
    "                               total_amount",
    "                               created_at",
    "                                    │",
    "                                    └──1──────< [BILL_ITEMS]",
    "                                                   id (PK)",
    "                                                   bill_id (FK)",
    "                                                   product_id (FK) ──> [PRODUCTS]",
    "                                                   quantity               id (PK)",
    "                                                   price                  name",
    "                                                   tax_amount             barcode",
    "                                                   line_total             price",
    "                                                                          quantity",
    "                                                                          category_id (FK) > [CATEGORIES]",
    "  [BILLS] ──1──> [PAYMENT]",
    "                   method (Cash/UPI/Card)",
    "                   status (Completed/Refunded)",
    "",
    "  [COUPONS] ──────────────────────────────> applied to [BILLS]",
    "    id (PK)",
    "    code (Unique)",
    "    discount_percentage",
    "    expiry_date",
])

add_heading(doc, "Entity Summary", level=2)
make_table(doc,
    headers=["Entity", "Service Ownership", "Key Fields"],
    rows=[
        ["Users",      "AuthService",     "id, email, passwordHash, roleId, storeId"],
        ["Products",   "ProductService",  "id, name, barcode, price, taxPercentage, quantity, categoryId"],
        ["Categories", "ProductService",  "id, name"],
        ["Carts",      "BillingService",  "id, userId, status, createdAt"],
        ["CartItems",  "BillingService",  "id, cartId, productId, quantity"],
        ["Bills",      "BillingService",  "id, userId, shiftId, totalAmount, createdAt"],
        ["BillItems",  "BillingService",  "id, billId, productId, productName, quantity, price, taxAmount, lineTotal"],
        ["Shifts",     "BillingService",  "id, userId, openedAt, closedAt, startingFloat, expectedCash, actualCash, variance, status"],
        ["Payments",   "BillingService",  "id, billId, method, amount, status"],
        ["Coupons",    "BillingService",  "id, code, discountPercentage, expiryDate"],
    ],
    col_widths=[3, 4, 9.5]
)
doc.add_paragraph()
doc.add_page_break()


# ════════════════════════════════════════════════════════════════════════════
# 6. USE CASE DIAGRAM
# ════════════════════════════════════════════════════════════════════════════
add_heading(doc, "6. Use Case Diagram")
add_divider(doc)

add_diagram_note(doc, "RetailPOS Use Case Diagram", [
    "",
    "  ┌─────────────────────────────────────────────────────────────────┐",
    "  │                        RetailPOS System                         │",
    "  │                                                                 │",
    "  │   ┌──────────────────────────────┐                             │",
    "  │   │  ● Login / Logout            │ ◄── [Admin] & [Cashier]    │",
    "  │   │  ● View Dashboard            │                             │",
    "  │   └──────────────────────────────┘                             │",
    "  │                                                                 │",
    "  │   [Cashier Only]                  [Admin Only]                 │",
    "  │   ─────────────                   ────────────                 │",
    "  │   ● Open / Close Shift            ● View Analytics Dashboard   │",
    "  │   ● Scan Barcode / Add to Cart    ● View All Shift Records     │",
    "  │   ● Apply Coupon Code             ● Create/Manage Products     │",
    "  │   ● Select Payment (Cash/UPI/Card)● Create Coupons             │",
    "  │   ● Complete Checkout             ● Mark Bills Paid / Refund   │",
    "  │   ● Print Invoice / Receipt       ● Direct Bill Composer       │",
    "  │   ● View Own Order History        ● View All Order History     │",
    "  │   ● View Notifications            ● Manage Categories          │",
    "  │                                                                 │",
    "  └─────────────────────────────────────────────────────────────────┘",
    "",
    "  Actors:",
    "  [Admin]   ─  Can do everything a Cashier does PLUS management functions.",
    "  [Cashier] ─  Limited to operational tasks: sales, shifts, own orders.",
])

doc.add_page_break()


# ════════════════════════════════════════════════════════════════════════════
# 7. WORKFLOW FLOWCHART
# ════════════════════════════════════════════════════════════════════════════
add_heading(doc, "7. End-to-End Workflow")
add_divider(doc)

add_diagram_note(doc, "Complete Daily Operational Workflow", [
    "",
    "  START DAY",
    "      │",
    "      ▼",
    "  [Login] ──> AuthService validates credentials ──> JWT Token issued",
    "      │",
    "      ▼",
    "  Is Shift Open?",
    "  ├── NO  ──> [Open Shift] Enter Starting Float ──> Shift Created in DB",
    "  └── YES ──> Proceed to Dashboard",
    "      │",
    "      ▼",
    "  ─── SALES CYCLE ─────────────────────────────────────────────────",
    "  │",
    "  │  1. Add Products to Cart  (Barcode Scan OR Manual Search)",
    "  │       └─> Angular Signals instantly recalculate: Subtotal + Tax + Total",
    "  │",
    "  │  2. Apply Coupon (Optional)",
    "  │       └─> BillingService validates coupon code, applies discount",
    "  │",
    "  │  3. Initiate Checkout  →  Select Payment Method",
    "  │       ├── Cash  →  Enter Amount Received → System shows Change to Return",
    "  │       ├── UPI   →  Enter UPI ID",
    "  │       └── Card  →  Enter Card Number (last 4)",
    "  │",
    "  │  4. Complete Sale",
    "  │       ├── BillingService saves Bill + BillItems to SQL Server",
    "  │       ├── BillingService publishes OrderCompletedEvent to RabbitMQ",
    "  │       ├── ProductService: Decrements inventory stock quantity",
    "  │       └── NotificationService: Saves notification to user inbox",
    "  │",
    "  │  5. Print Thermal Receipt (IST Timezone, Black & White formatted)",
    "  │",
    "  └──────────────────────────────────────────────────────────────────",
    "      │",
    "      ▼",
    "  ─── END OF DAY ────────────────────────────────────────────────────",
    "  │",
    "  │  Admin views Analytics Dashboard",
    "  │       ├── Revenue Breakdown (Today vs Total) - Doughnut Chart",
    "  │       ├── Top Selling Products - Bar Chart",
    "  │       └── Low Stock Watchlist - with progress bars",
    "  │",
    "  │  Cashier closes Shift",
    "  │       ├── Enters Actual Cash in drawer",
    "  │       ├── System computes Variance (Actual vs Expected)",
    "  │       └── Shift Record saved for Admin Audit",
    "  └──────────────────────────────────────────────────────────────────",
    "      │",
    "      ▼",
    "  END DAY",
])
doc.add_page_break()


# ════════════════════════════════════════════════════════════════════════════
# 8. FEATURE MATRIX
# ════════════════════════════════════════════════════════════════════════════
add_heading(doc, "8. Feature-to-Technology Matrix")
add_divider(doc)
make_table(doc,
    headers=["Feature", "Technology / Pattern", "File Location"],
    rows=[
        ["User Authentication",     "JWT + ASP.NET Core Identity",       "AuthService/Controllers/AuthController.cs"],
        ["Role-Based UI",           "Angular Signals + AuthStore",        "src/app/core/store/auth.store.ts"],
        ["Route Guards (RBAC)",     "Angular CanActivate Guard",          "src/app/core/guards/role.guard.ts"],
        ["Barcode Scanning",        "Global Keyboard Event Listener",     "src/app/features/cart/cart-page.component.ts"],
        ["Cart Totals (Realtime)",  "Angular Computed Signals",           "src/app/features/cart/cart-page.component.ts"],
        ["Cash Calculator",         "Angular Computed Signal",            "src/app/features/cart/cart-page.component.ts"],
        ["Coupon Validation",       "REST API + EF Core DB Lookup",       "BillingService/Services/BillingService.cs"],
        ["Checkout & Invoice",      ".NET 8 Service + EF Core",           "BillingService/Services/BillingService.cs"],
        ["Shift Management",        ".NET 8 + SQL Server",                "BillingService/Services/ShiftService.cs"],
        ["Async Inventory Update",  "RabbitMQ + MassTransit Consumer",   "ProductService/Consumers/"],
        ["Notifications",           "RabbitMQ Event Consumer",            "NotificationService/Services/"],
        ["Analytics Charts",        "Chart.js (Doughnut + Bar)",          "src/app/features/admin/admin-page.component.ts"],
        ["Thermal Receipt Print",   "@media print CSS + Date Pipe (+0530)","invoice-view.component.scss/.html"],
        ["Admin Dashboard BFF",     "AdminService Aggregator Pattern",     "AdminService/Services/AdminService.cs"],
        ["Global Toasts",           "Angular Signal-based Toast Service",  "src/app/shared/services/toast.service.ts"],
    ],
    col_widths=[4.5, 5, 7]
)
doc.add_paragraph()


# ════════════════════════════════════════════════════════════════════════════
# SAVE
# ════════════════════════════════════════════════════════════════════════════
output_path = "/Users/krishnasharma/Desktop/RetailPOSF copy 2/docs/RetailPOS_Project_Documentation.docx"
doc.save(output_path)
print(f"SUCCESS: {output_path}")
