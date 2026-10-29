# Quick Start Guide - PaymentService

## Prerequisites

- .NET 8 SDK installed
- SQL Server running
- IdentityService running (to get JWT tokens)
- OrderService running (to have orders to pay for)

## Steps to Run

### 1. Update Database Connection String

Edit `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CropDealPayments;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
}
```

### 2. Ensure JWT Settings Match Other Services

The JWT Key, Issuer, and Audience MUST match IdentityService exactly:

```json
"Jwt": {
  "Key": "your-very-secure-secret-key-minimum-32-characters-long",
  "Issuer": "CropDealIdentityService",
  "Audience": "CropDeal"
}
```

### 3. Apply Migrations

```bash
cd PaymentService
dotnet ef database update
```

### 4. Run the Application

```bash
dotnet run
```

### 5. Open Swagger UI

```
https://localhost:<port>/swagger
```

## Test the API

### Prerequisites: Complete Order Flow

Before testing PaymentService, you need an order:

1. **From IdentityService:**
   - Register Dealer and Farmer users
   - Get JWT tokens

2. **From CropService:**
   - Create crops (as Admin)
   - Create crop listings (as Farmer)

3. **From OrderService:**
   - Create order (as Dealer)
   - Note the OrderId, FarmerId, and Amount

### Step 1: Get JWT Token

From IdentityService Swagger:

```json
// Login as Dealer
POST /api/auth/login
{
  "email": "dealer@example.com",
  "password": "DealerPass123"
}
```

Save the Dealer token.

### Step 2: Authorize in PaymentService Swagger

1. Click **Authorize** button at the top
2. Enter: `Bearer <your-dealer-token>`
3. Click **Authorize**

### Step 3: Process Payment (Dealer)

```json
POST /api/payment
{
  "orderId": 1,
  "farmerId": 5,
  "amount": 125000.00,
  "paymentMethod": "CreditCard"
}
```

**Payment Methods Examples:**
- "CreditCard"
- "DebitCard"
- "UPI"
- "NetBanking"
- "Cash"

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

**Note:** Payment status 1 means Successful.

### Step 4: View Payment History (Dealer)

```
GET /api/payment/history
```

This returns all payments made by the authenticated dealer.

### Step 5: View Payment Details

```
GET /api/payment/1
```

Returns full payment information including transaction reference.

### Step 6: View Farmer Payment History

1. Click **Authorize** again
2. Enter: `Bearer <your-farmer-token>`
3. Click **Authorize**

```
GET /api/payment/history
```

This returns all payments received by the authenticated farmer.

### Step 7: Refund Payment (Admin)

1. Get Admin JWT token from IdentityService
2. Click **Authorize** and enter Admin token

```
PUT /api/payment/1/refund
```

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

**Note:** Payment status 3 means Refunded.

## Complete Test Flow

```bash
# 1. Dealer processes payment
POST /api/payment (Dealer token)
Body: { "orderId": 1, "farmerId": 5, "amount": 125000, "paymentMethod": "CreditCard" }

# 2. Dealer views payment history
GET /api/payment/history (Dealer token)

# 3. Farmer views received payments
GET /api/payment/history (Farmer token)

# 4. View specific payment details
GET /api/payment/1 (Any authenticated user)

# 5. Admin refunds payment
PUT /api/payment/1/refund (Admin token)
```

## Full CropDeal Workflow

Here's the complete end-to-end workflow across all services:

### 1. User Registration (IdentityService)
```
Register Dealer: POST /api/auth/register
Register Farmer: POST /api/auth/register
Login: POST /api/auth/login
```

### 2. Crop Setup (CropService)
```
Admin creates crop: POST /api/crop
Farmer creates listing: POST /api/crops/listings
```

### 3. Order Creation (OrderService)
```
Dealer places order: POST /api/order
Farmer confirms order: PUT /api/order/{id}/status
```

### 4. Payment Processing (PaymentService)
```
Dealer makes payment: POST /api/payment
Payment status: Successful
```

### 5. Order Completion (OrderService)
```
Update order status: PUT /api/order/{id}/status (Completed)
Generate invoice: POST /api/invoice/generate/{orderId}
Generate receipt: POST /api/receipt/generate/{orderId}
```

### 6. Review (OrderService)
```
Dealer submits review: POST /api/review
```

## What's Working

✅ Payment processing (Dealer only)  
✅ Duplicate payment prevention (one per order)  
✅ Payment history (role-specific)  
✅ Payment details viewing  
✅ Refund processing (Admin only)  
✅ Transaction reference generation  
✅ JWT authentication and authorization  
✅ Simulated payment gateway  
✅ Swagger integration  

## Project Structure

```
PaymentService/
├── Controllers/
│   └── PaymentController.cs         # Payment endpoints
├── DTOs/
│   ├── CreatePaymentDto.cs
│   ├── PaymentResponseDto.cs
│   └── PaymentHistoryDto.cs
├── Data/
│   └── AppDbContext.cs              # EF Core DbContext
├── Models/
│   ├── Payment.cs                   # Payment entity
│   └── PaymentStatus.cs             # Status enum
├── Repositories/
│   ├── IPaymentRepository.cs
│   └── PaymentRepository.cs
├── Services/
│   ├── IPaymentService.cs
│   └── PaymentService.cs
├── Migrations/                       # EF Core migrations
├── Program.cs                        # Application startup
├── appsettings.json                 # Configuration
└── README.md                         # Full documentation
```

## Troubleshooting

### JWT Authentication Fails

**Problem:** Getting 401 Unauthorized  
**Solution:**
- Verify JWT Key matches IdentityService exactly
- Check Issuer and Audience values match
- Ensure token format is: `Bearer <token>`
- Verify token hasn't expired

### Cannot Process Payment

**Problem:** "Payment already exists for this order"  
**Solution:**
- Each order can only have one payment
- Check payment history to see if payment was already made
- Use a different order ID

### Refund Fails

**Problem:** "Only successful payments can be refunded"  
**Solution:**
- Payment must have status: Successful (1)
- Failed or Pending payments cannot be refunded
- Already refunded payments cannot be refunded again

### Cannot View Payment History

**Problem:** Empty history or 403 Forbidden  
**Solution:**
- Ensure you're authenticated with correct role
- Dealers see payments they made
- Farmers see payments they received
- Check your JWT token has correct UserId and Role

## Sample Data for Testing

### Create Multiple Payments

```json
// Payment 1
{ "orderId": 1, "farmerId": 5, "amount": 125000.00, "paymentMethod": "CreditCard" }

// Payment 2
{ "orderId": 2, "farmerId": 5, "amount": 220000.00, "paymentMethod": "DebitCard" }

// Payment 3
{ "orderId": 3, "farmerId": 6, "amount": 1500.00, "paymentMethod": "UPI" }
```

## Payment Status Values

- **0 = Pending**: Payment initiated but not processed
- **1 = Successful**: Payment completed successfully
- **2 = Failed**: Payment processing failed
- **3 = Refunded**: Payment was refunded

## Transaction Reference Format

```
TXN-{yyyyMMddHHmmss}-{8-char-random}

Examples:
TXN-20261029123045-A1B2C3D4
TXN-20261029140230-F7E8D9C0
```

## Integration Points

### IdentityService
- Provides JWT tokens for authentication
- DealerId and FarmerId reference users
- No direct API calls

### OrderService
- OrderId references orders
- Payment triggers order completion
- No direct API calls

### CropService
- No direct integration
- Orders reference crop listings

## Important Notes

1. **Fake Payment Gateway**: PaymentService uses simulated payment processing for testing
2. **No Card Storage**: Credit/debit card numbers are NOT stored
3. **One Payment Per Order**: Duplicate payment prevention is enforced
4. **Role-Based Access**: Dealers make payments, Farmers receive, Admin refunds
5. **Transaction References**: Unique identifiers for tracking

## Next Steps

Once PaymentService is working correctly:
1. Test complete order-to-payment flow
2. Verify payment history for different roles
3. Test refund functionality
4. Consider integrating real payment gateway (Stripe, PayPal, etc.)
5. Add payment notifications
6. Implement payment webhooks if needed

## Real Payment Gateway Integration

To integrate with a real payment gateway:

### Option 1: Stripe
```bash
dotnet add package Stripe.net
```

### Option 2: PayPal
```bash
dotnet add package PayPal
```

### Option 3: Razorpay (for India)
```bash
dotnet add package Razorpay.net
```

Update the `SimulatePaymentGateway` method in `PaymentService.cs` to call the actual gateway API.

## All Four Services Complete!

Congratulations! All four CropDeal microservices are now complete:

1. ✅ **IdentityService** - Authentication & Users
2. ✅ **CropService** - Crops & Listings
3. ✅ **OrderService** - Orders, Invoices, Receipts & Reviews
4. ✅ **PaymentService** - Payment Processing & Transactions

The complete CropDeal backend is ready for testing!
