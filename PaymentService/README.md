# CropDeal - Payment Service

Payment Service handles payment processing and transaction management for the CropDeal system.

## Technology Stack

- C# / ASP.NET Core 8 Web API
- Entity Framework Core 8
- SQL Server
- JWT Bearer Authentication
- Swagger / OpenAPI

## Project Structure

```
PaymentService
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

### Payment
Represents a payment transaction for an order.

- **Id**: Unique identifier
- **OrderId**: Reference to Order (from OrderService)
- **DealerId**: Dealer who made the payment (from IdentityService)
- **FarmerId**: Farmer who receives the payment (from IdentityService)
- **Amount**: Payment amount
- **PaymentMethod**: Payment method (e.g., "CreditCard", "DebitCard", "UPI")
- **PaymentStatus**: Pending, Successful, Failed, or Refunded
- **TransactionReference**: Unique transaction reference
- **CreatedAt**: Payment timestamp

### PaymentStatus Enum
- **Pending** (0): Payment initiated but not processed
- **Successful** (1): Payment completed successfully
- **Failed** (2): Payment processing failed
- **Refunded** (3): Payment refunded

## API Endpoints

### Payment Processing

#### POST /api/payment
Process a payment for an order (Dealer only).

**Authorization:** Bearer token with Dealer role

**Request Body:**
```json
{
  "orderId": 1,
  "farmerId": 5,
  "amount": 125000.00,
  "paymentMethod": "CreditCard"
}
```

**Notes:**
- DealerId is extracted from JWT token
- TransactionReference is auto-generated
- Payment status is set based on processing result
- One payment per order (prevents duplicate payments)

**Response (Success):**
```json
{
  "id": 1,
  "orderId": 1,
  "dealerId": 3,
  "farmerId": 5,
  "amount": 125000.00,
  "paymentMethod": "CreditCard",
  "paymentStatus": 1,
  "transactionReference": "TXN-20261029123045-A1B2C3D4",
  "createdAt": "2026-10-29T12:30:45Z"
}
```

**Response Codes:**
- 201 Created - Payment processed successfully
- 400 Bad Request - Invalid data or processing failed
- 401 Unauthorized - Not authenticated
- 409 Conflict - Payment already exists for order

#### GET /api/payment/history
Get payment history for authenticated user.

**Authorization:** Bearer token (Dealer, Farmer, or Admin)

**Behavior:**
- **Dealer**: Returns payments made by the dealer
- **Farmer**: Returns payments received by the farmer
- **Admin**: Returns all payments (implementation dependent)

**Response:**
```json
[
  {
    "id": 1,
    "orderId": 1,
    "amount": 125000.00,
    "paymentMethod": "CreditCard",
    "paymentStatus": 1,
    "transactionReference": "TXN-20261029123045-A1B2C3D4",
    "createdAt": "2026-10-29T12:30:45Z"
  }
]
```

**Status Values:**
- 0 = Pending
- 1 = Successful
- 2 = Failed
- 3 = Refunded

#### GET /api/payment/{id}
Get payment details by ID.

**Authorization:** Bearer token (any authenticated user)

**Response:**
```json
{
  "id": 1,
  "orderId": 1,
  "dealerId": 3,
  "farmerId": 5,
  "amount": 125000.00,
  "paymentMethod": "CreditCard",
  "paymentStatus": 1,
  "transactionReference": "TXN-20261029123045-A1B2C3D4",
  "createdAt": "2026-10-29T12:30:45Z"
}
```

**Response Codes:**
- 200 OK
- 401 Unauthorized
- 404 Not Found

#### PUT /api/payment/{id}/refund
Refund a successful payment (Admin only).

**Authorization:** Bearer token with Admin role

**Business Rules:**
- Only successful payments can be refunded
- Payment status changes to Refunded
- Cannot refund already refunded payments

**Response:**
```json
{
  "id": 1,
  "orderId": 1,
  "dealerId": 3,
  "farmerId": 5,
  "amount": 125000.00,
  "paymentMethod": "CreditCard",
  "paymentStatus": 3,
  "transactionReference": "TXN-20261029123045-A1B2C3D4",
  "createdAt": "2026-10-29T12:30:45Z"
}
```

**Response Codes:**
- 200 OK - Refund successful
- 400 Bad Request - Cannot refund (wrong status)
- 401 Unauthorized - Not admin
- 404 Not Found - Payment not found

## Database Schema

### Payments Table

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key (auto-increment) |
| OrderId | int | Reference to Order |
| DealerId | int | Reference to User (Dealer) |
| FarmerId | int | Reference to User (Farmer) |
| Amount | decimal(18,2) | Payment amount |
| PaymentMethod | nvarchar(50) | Payment method |
| PaymentStatus | int | 0=Pending, 1=Successful, 2=Failed, 3=Refunded |
| TransactionReference | nvarchar(100) | Unique transaction ID |
| CreatedAt | datetime2 | Payment timestamp (UTC) |

**Indexes:**
- Primary key on Id
- Index on OrderId
- Index on DealerId
- Index on FarmerId
- Unique index on TransactionReference

## Setup Instructions

### 1. Configure Database Connection

Update `appsettings.json` with your SQL Server connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CropDealPayments;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=True;"
}
```

### 2. Configure JWT Settings

Ensure JWT settings match other services (IdentityService):

```json
"Jwt": {
  "Key": "your-very-secure-secret-key-minimum-32-characters-long",
  "Issuer": "CropDealIdentityService",
  "Audience": "CropDeal"
}
```

**Important:** Use the SAME JWT Key as all other services!

### 3. Apply Database Migrations

```bash
dotnet ef database update
```

This creates the `CropDealPayments` database with the Payments table.

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

## Payment Workflow

### 1. Order Created (OrderService)
```
Dealer creates order
Order Status: Pending
```

### 2. Payment Processing (PaymentService)
```
Dealer initiates payment
Payment created with status: Pending
Payment gateway simulated (fake implementation)
If successful: Payment Status → Successful
If failed: Payment Status → Failed
```

### 3. Order Completion (OrderService)
```
After successful payment
Order Status → Completed
Invoice and Receipt generated
```

### 4. Review (OrderService)
```
Dealer submits review for completed order
```

### 5. Refund (if needed)
```
Admin can refund successful payments
Payment Status → Refunded
```

## Payment Processing

### Fake Implementation

For this evaluation project, PaymentService uses a **simulated payment gateway**:

```csharp
private bool SimulatePaymentGateway(Payment payment)
{
    // Fake payment processing
    // Always returns true for testing
    return true;
}
```

**Transaction Reference Format:**
```
TXN-{timestamp}-{random}
Example: TXN-20261029123045-A1B2C3D4
```

### Real Payment Gateway Integration

To integrate with a real payment gateway (Stripe, PayPal, Razorpay, etc.), update the `SimulatePaymentGateway` method:

```csharp
private async Task<bool> ProcessRealPayment(Payment payment)
{
    // Example: Stripe integration
    var paymentIntent = await _stripeService.CreatePaymentIntent(
        payment.Amount,
        payment.PaymentMethod
    );
    
    return paymentIntent.Status == "succeeded";
}
```

## Authorization Rules

| Endpoint | Access | Notes |
|----------|--------|-------|
| POST /api/payment | Dealer only | Process payment |
| GET /api/payment/history | Authenticated | Role-specific history |
| GET /api/payment/{id} | Authenticated | Any user can view |
| PUT /api/payment/{id}/refund | Admin only | Refund management |

## Security Features

- **JWT Authentication**: Same configuration as other services
- **Role-Based Authorization**: Dealer, Farmer, Admin roles enforced
- **DealerId from JWT**: Cannot be spoofed by client
- **Duplicate Prevention**: One payment per order
- **Transaction Reference**: Unique identifier for each payment
- **No Sensitive Data**: Card numbers are NOT stored

## Integration with Other Services

### IdentityService
- Provides JWT tokens
- DealerId and FarmerId are user IDs from IdentityService
- No direct API calls (logical references only)

### OrderService
- OrderId references an order
- After successful payment, OrderService should update order status
- No direct API calls (logical references only)

**Note:** PaymentService does NOT call other services' APIs. It stores IDs as references and validates JWT tokens independently.

## Business Logic

### Payment Creation
1. Validate request (amount > 0, required fields)
2. Check for duplicate payment (one per order)
3. Extract DealerId from JWT
4. Generate unique transaction reference
5. Create payment record with status: Pending
6. Simulate payment processing
7. Update status to Successful or Failed
8. Return payment response

### Payment History
- Dealers see their payment history
- Farmers see payments they received
- Filtered by user ID from JWT

### Refund Processing
1. Find payment by ID
2. Validate payment status (must be Successful)
3. Update status to Refunded
4. Return updated payment

## Validation Rules

### Payment Creation
- OrderId required
- FarmerId required
- Amount must be > 0
- PaymentMethod required
- One payment per order

### Refund
- Payment must exist
- Payment status must be Successful
- Cannot refund already refunded payments

## Common Issues

### JWT Authentication Fails
**Problem:** Getting 401 Unauthorized

**Solution:**
- Verify JWT Key matches IdentityService exactly
- Check Issuer and Audience values match
- Ensure token format is: `Bearer <token>`
- Verify token hasn't expired

### Cannot Process Payment
**Problem:** Duplicate payment error

**Solution:**
- Each order can have only one payment
- Check if payment already exists for the order
- Use GET /api/payment/history to view existing payments

### Refund Fails
**Problem:** Cannot refund payment

**Solution:**
- Only Admin can refund
- Payment must have status: Successful
- Cannot refund Failed or already Refunded payments

## Development Notes

- PaymentService uses its own database (CropDealPayments)
- OrderId, DealerId, FarmerId are logical references (no FK constraints)
- All timestamps use UTC
- Decimal precision is 18,2 for monetary values
- Transaction references are unique with timestamp + random
- Payment gateway is simulated (no real card processing)

## Future Enhancements

### Real Payment Gateway
- Integrate Stripe, PayPal, or Razorpay
- Handle payment webhooks
- Support multiple payment methods
- Implement retry logic for failed payments

### Advanced Features
- Partial refunds
- Payment history export
- Transaction receipts
- Payment notifications
- Fraud detection
- Payment analytics

## Testing Workflow

### Prerequisites
- IdentityService running (for JWT tokens)
- OrderService running (to create orders)
- SQL Server running

### Test Flow
1. Get Dealer JWT token from IdentityService
2. Create order in OrderService
3. Process payment for the order
4. View payment history
5. Get payment details
6. Admin can refund if needed

## Next Steps

After PaymentService is working:
1. Test payment processing flow
2. Verify JWT authentication
3. Test payment history for different roles
4. Test refund functionality
5. Consider adding real payment gateway integration
6. Implement cross-service communication if needed
