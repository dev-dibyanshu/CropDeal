# CropDeal - Order Service

Order Service manages orders, invoices, receipts, and reviews for the CropDeal system.

## Technology Stack

- C# / ASP.NET Core 8 Web API
- Entity Framework Core 8
- SQL Server
- JWT Bearer Authentication
- Swagger / OpenAPI

## Project Structure

```
OrderService
│
├── Controllers/        # API endpoints
├── DTOs/              # Data transfer objects
├── Data/              # DbContext
├── Migrations/        # EF Core migrations
├── Models/            # Domain models
├── Repositories/      # Data access layer
├── Services/          # Business logic
└── Properties/        # Project properties
```

## Domain Models

### Order
Represents a purchase order from a dealer.

- **Id**: Unique identifier
- **ListingId**: Reference to CropListing (from CropService)
- **DealerId**: Dealer who placed the order (from IdentityService)
- **FarmerId**: Farmer who receives the order (from IdentityService)
- **Quantity**: Quantity ordered
- **Price**: Price per unit
- **TotalAmount**: Total cost (Quantity × Price)
- **Status**: Pending, Confirmed, Cancelled, or Completed
- **CreatedAt**: Order creation timestamp

### Invoice
Generated for completed orders.

- **Id**: Unique identifier
- **OrderId**: Reference to Order
- **InvoiceNumber**: Unique invoice number
- **InvoiceDate**: Invoice generation date
- **TotalAmount**: Total invoice amount

### Receipt
Generated for completed orders with payment.

- **Id**: Unique identifier
- **OrderId**: Reference to Order
- **ReceiptNumber**: Unique receipt number
- **ReceiptDate**: Receipt generation date
- **DocumentUrl**: URL/path to receipt document

### Review
Customer feedback for completed orders.

- **Id**: Unique identifier
- **OrderId**: Reference to Order
- **DealerId**: Dealer who submitted the review
- **FarmerId**: Farmer being reviewed
- **Rating**: Rating from 1 to 5
- **Comment**: Review text
- **CreatedAt**: Review submission timestamp

## API Endpoints

### Order Management

#### POST /api/order
Create a new order (Dealer only).

**Authorization:** Bearer token with Dealer role

**Request Body:**
```json
{
  "listingId": 1,
  "farmerId": 5,
  "quantity": 50.00,
  "price": 2500.00
}
```

**Notes:**
- DealerId is extracted from JWT token
- TotalAmount is automatically calculated
- Status is set to Pending
- CreatedAt is set to current UTC time

**Response:**
- 201 Created
- 400 Bad Request
- 401 Unauthorized

#### GET /api/order/my-orders
Get orders placed by the authenticated dealer.

**Authorization:** Bearer token with Dealer role

**Response:**
```json
[
  {
    "id": 1,
    "listingId": 1,
    "dealerId": 3,
    "farmerId": 5,
    "quantity": 50.00,
    "price": 2500.00,
    "totalAmount": 125000.00,
    "status": 0,
    "createdAt": "2026-10-29T12:00:00Z"
  }
]
```

**Status Values:**
- 0 = Pending
- 1 = Confirmed
- 2 = Cancelled
- 3 = Completed

#### GET /api/order/received
Get orders received by the authenticated farmer.

**Authorization:** Bearer token with Farmer role

**Response:**
Same format as my-orders endpoint

#### GET /api/order/{id}
Get order details by ID.

**Authorization:** Bearer token (any authenticated user)

**Response:**
```json
{
  "id": 1,
  "listingId": 1,
  "dealerId": 3,
  "farmerId": 5,
  "quantity": 50.00,
  "price": 2500.00,
  "totalAmount": 125000.00,
  "status": 0,
  "createdAt": "2026-10-29T12:00:00Z"
}
```

#### PUT /api/order/{id}/cancel
Cancel an order (Dealer can only cancel their own orders).

**Authorization:** Bearer token with Dealer role

**Response:**
- 200 OK
- 400 Bad Request (already cancelled or completed)
- 401 Unauthorized
- 403 Forbidden (not order owner)
- 404 Not Found

#### PUT /api/order/{id}/status
Update order status (Admin or Farmer).

**Authorization:** Bearer token with Admin or Farmer role

**Request Body:**
```json
{
  "status": 1
}
```

**Response:**
- 200 OK
- 400 Bad Request
- 401 Unauthorized
- 404 Not Found

### Invoice Management

#### GET /api/invoice/{id}
Get invoice details by ID.

**Authorization:** Bearer token (any authenticated user)

**Response:**
```json
{
  "id": 1,
  "orderId": 1,
  "invoiceNumber": "INV-000001-20261029",
  "invoiceDate": "2026-10-29T12:00:00Z",
  "totalAmount": 125000.00
}
```

#### POST /api/invoice/generate/{orderId}
Generate invoice for a completed order (Admin or Farmer).

**Authorization:** Bearer token with Admin or Farmer role

**Response:**
- 201 Created
- 400 Bad Request (order not completed or invoice already exists)
- 404 Not Found

### Receipt Management

#### GET /api/receipt/{id}
Get receipt details by ID.

**Authorization:** Bearer token (any authenticated user)

**Response:**
```json
{
  "id": 1,
  "orderId": 1,
  "receiptNumber": "REC-000001-20261029",
  "receiptDate": "2026-10-29T12:00:00Z",
  "documentUrl": "/receipts/1.pdf"
}
```

#### POST /api/receipt/generate/{orderId}
Generate receipt for a completed order (Admin or Farmer).

**Authorization:** Bearer token with Admin or Farmer role

**Response:**
- 201 Created
- 400 Bad Request (order not completed or receipt already exists)
- 404 Not Found

### Review Management

#### POST /api/review
Submit a review for a completed order (Dealer only).

**Authorization:** Bearer token with Dealer role

**Request Body:**
```json
{
  "orderId": 1,
  "farmerId": 5,
  "rating": 5,
  "comment": "Excellent quality crops, delivered on time!"
}
```

**Validation:**
- Rating must be between 1 and 5
- Order must exist and be completed
- Dealer must own the order
- Only one review per order

**Response:**
- 201 Created
- 400 Bad Request
- 401 Unauthorized

#### GET /api/review/farmer/{farmerId}
Get all reviews for a specific farmer (public access).

**Authorization:** None required

**Response:**
```json
[
  {
    "id": 1,
    "orderId": 1,
    "dealerId": 3,
    "farmerId": 5,
    "rating": 5,
    "comment": "Excellent quality crops, delivered on time!",
    "createdAt": "2026-10-29T12:00:00Z"
  }
]
```

## Database Schema

### Orders Table

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key (auto-increment) |
| ListingId | int | Reference to CropListing |
| DealerId | int | Reference to User (Dealer) |
| FarmerId | int | Reference to User (Farmer) |
| Quantity | decimal(18,2) | Quantity ordered |
| Price | decimal(18,2) | Price per unit |
| TotalAmount | decimal(18,2) | Total order amount |
| Status | int | 0=Pending, 1=Confirmed, 2=Cancelled, 3=Completed |
| CreatedAt | datetime2 | Order creation timestamp (UTC) |

### Invoices Table

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key (auto-increment) |
| OrderId | int | Foreign key to Orders |
| InvoiceNumber | nvarchar(50) | Unique invoice number |
| InvoiceDate | datetime2 | Invoice generation date (UTC) |
| TotalAmount | decimal(18,2) | Invoice amount |

### Receipts Table

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key (auto-increment) |
| OrderId | int | Foreign key to Orders |
| ReceiptNumber | nvarchar(50) | Unique receipt number |
| ReceiptDate | datetime2 | Receipt generation date (UTC) |
| DocumentUrl | nvarchar(500) | URL/path to receipt document |

### Reviews Table

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key (auto-increment) |
| OrderId | int | Foreign key to Orders |
| DealerId | int | Reference to User (Dealer) |
| FarmerId | int | Reference to User (Farmer) |
| Rating | int | Rating (1-5) |
| Comment | nvarchar(1000) | Review text |
| CreatedAt | datetime2 | Review timestamp (UTC) |

**Relationships:**
- Order → Invoice (one-to-one)
- Order → Receipt (one-to-one)
- Order → Reviews (one-to-many)

## Setup Instructions

### 1. Configure Database Connection

Update `appsettings.json` with your SQL Server connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CropDealOrders;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=True;"
}
```

### 2. Configure JWT Settings

Ensure JWT settings match IdentityService:

```json
"Jwt": {
  "Key": "your-very-secure-secret-key-minimum-32-characters-long",
  "Issuer": "CropDealIdentityService",
  "Audience": "CropDeal"
}
```

**Important:** Use the SAME JWT Key as IdentityService and CropService!

### 3. Apply Database Migrations

```bash
dotnet ef database update
```

This creates the `CropDealOrders` database with all tables.

### 4. Run the Application

```bash
dotnet run
```

The API will start on:
- HTTPS: https://localhost:7XXX
- HTTP: http://localhost:5XXX

(Check console output for exact ports)

### 5. Test with Swagger

1. Navigate to: https://localhost:7XXX/swagger
2. Get JWT tokens from IdentityService (Dealer and Farmer)
3. Click **Authorize** button
4. Enter: `Bearer <your-token>`
5. Test the endpoints

## Order Workflow

### 1. Create Order (Dealer)
```
Dealer browses CropListing → Places order
Status: Pending
```

### 2. Confirm Order (Farmer/Admin)
```
Farmer confirms order availability
Status: Confirmed
```

### 3. Payment (PaymentService - separate)
```
Payment processing happens in PaymentService
```

### 4. Complete Order
```
After successful payment
Status: Completed
```

### 5. Generate Documents
```
Invoice generated (for farmer)
Receipt generated (for dealer)
```

### 6. Submit Review (Dealer)
```
Dealer reviews the transaction
Rating: 1-5 stars
```

## Authorization Rules

| Endpoint | Access |
|----------|--------|
| POST /api/order | Dealer only |
| GET /api/order/my-orders | Dealer only |
| GET /api/order/received | Farmer only |
| GET /api/order/{id} | Authenticated users |
| PUT /api/order/{id}/cancel | Dealer (own orders only) |
| PUT /api/order/{id}/status | Admin, Farmer |
| GET /api/invoice/{id} | Authenticated users |
| POST /api/invoice/generate/{orderId} | Admin, Farmer |
| GET /api/receipt/{id} | Authenticated users |
| POST /api/receipt/generate/{orderId} | Admin, Farmer |
| POST /api/review | Dealer only |
| GET /api/review/farmer/{farmerId} | Public |

## Security Features

- **JWT Authentication**: Same configuration as other services
- **Role-Based Authorization**: Dealer, Farmer, Admin roles enforced
- **Ownership Validation**: Users can only manage their own orders
- **Status Validation**: Business rules for order lifecycle
- **Review Integrity**: One review per order, only for completed orders

## Integration with Other Services

### IdentityService
- Provides JWT tokens
- DealerId and FarmerId are user IDs from IdentityService
- No direct API calls (logical references only)

### CropService
- ListingId references a CropListing
- No direct API calls (logical references only)

### PaymentService (Future)
- Will process payments for orders
- Will update order status to Completed after successful payment

**Note:** OrderService does NOT call other services' APIs. It stores IDs as references and validates JWT tokens independently.

## Common Issues

### JWT Authentication Fails
**Problem:** Getting 401 Unauthorized

**Solution:**
- Verify JWT Key matches IdentityService exactly
- Check Issuer and Audience values match
- Ensure token format is: `Bearer <token>`
- Verify token hasn't expired

### Cannot Create Order
**Problem:** Authorization errors

**Solution:**
- Ensure you're logged in as Dealer
- Check JWT token contains correct role
- Verify UserId claim is present

### Cannot Update Status
**Problem:** 403 Forbidden

**Solution:**
- Only Admin and Farmer can update status
- Dealers can only cancel their own orders
- Check your role in JWT token

### Invoice/Receipt Generation Fails
**Problem:** Cannot generate documents

**Solution:**
- Order must have Status = Completed
- Can only generate once per order
- Check order exists and status is correct

### Review Submission Fails
**Problem:** Cannot submit review

**Solution:**
- Order must be completed
- Dealer must own the order
- Only one review per order allowed
- Rating must be 1-5

## Development Notes

- OrderService uses its own database (CropDealOrders)
- ListingId, DealerId, FarmerId are logical references (no FK constraints)
- All timestamps use UTC
- Decimal precision is 18,2 for monetary values
- Invoice/Receipt numbers are auto-generated with order ID and date
- Review ratings are validated to be between 1 and 5

## Next Steps

After OrderService is working:
1. Create PaymentService for payment processing
2. Integrate order status updates with payments
3. Implement actual invoice/receipt PDF generation
4. Add order notifications/emails
5. Implement API Gateway for unified access
