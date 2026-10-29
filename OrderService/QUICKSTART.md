# Quick Start Guide - OrderService

## Prerequisites

- .NET 8 SDK installed
- SQL Server running
- IdentityService running (to get JWT tokens)
- CropService running (to have crop listings)

## Steps to Run

### 1. Update Database Connection String

Edit `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CropDealOrders;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
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
cd OrderService
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

### Prerequisites: Setup from Other Services

Before testing OrderService, you need:

1. **From IdentityService:**
   - Register users (Dealer and Farmer)
   - Get JWT tokens

2. **From CropService:**
   - Create crops (as Admin)
   - Create crop listings (as Farmer)

### Step 1: Get JWT Tokens

From IdentityService Swagger:

```json
// Login as Dealer
POST /api/auth/login
{
  "email": "dealer@example.com",
  "password": "DealerPass123"
}
// Save the Dealer token

// Login as Farmer
POST /api/auth/login
{
  "email": "farmer@example.com",
  "password": "FarmerPass123"
}
// Save the Farmer token
```

### Step 2: Create Crop Listings (if not done)

From CropService Swagger (use Farmer token):

```json
POST /api/crops/listings
{
  "cropId": 1,
  "quantity": 100.00,
  "expectedPrice": 2500.00,
  "location": "Punjab, India"
}
```

Note the ListingId and FarmerId from the response.

### Step 3: Authorize in OrderService Swagger

1. Click **Authorize** button at the top
2. Enter: `Bearer <your-dealer-token>`
3. Click **Authorize**

### Step 4: Create an Order (Dealer)

```json
POST /api/order
{
  "listingId": 1,
  "farmerId": 5,
  "quantity": 50.00,
  "price": 2500.00
}
```

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

### Step 5: View Dealer's Orders

```
GET /api/order/my-orders
```

This returns all orders placed by the authenticated dealer.

### Step 6: View Farmer's Orders

1. Click **Authorize** again
2. Enter: `Bearer <your-farmer-token>`
3. Click **Authorize**

```
GET /api/order/received
```

This returns all orders received by the authenticated farmer.

### Step 7: Update Order Status (Farmer)

Farmer confirms the order:

```json
PUT /api/order/1/status
{
  "status": 1
}
```

**Status Values:**
- 0 = Pending
- 1 = Confirmed
- 2 = Cancelled
- 3 = Completed

### Step 8: Complete the Order

After payment is processed (in PaymentService):

```json
PUT /api/order/1/status
{
  "status": 3
}
```

### Step 9: Generate Invoice (Farmer/Admin)

```
POST /api/invoice/generate/1
```

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

### Step 10: Generate Receipt (Farmer/Admin)

```
POST /api/receipt/generate/1
```

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

### Step 11: Submit Review (Dealer)

1. Click **Authorize** again
2. Enter: `Bearer <your-dealer-token>`
3. Click **Authorize**

```json
POST /api/review
{
  "orderId": 1,
  "farmerId": 5,
  "rating": 5,
  "comment": "Excellent quality crops, delivered on time!"
}
```

### Step 12: View Farmer Reviews (Public)

No authentication needed:

```
GET /api/review/farmer/5
```

Returns all reviews for farmer with ID 5.

### Step 13: Cancel Order (Dealer)

Dealers can cancel their own orders before completion:

```
PUT /api/order/1/cancel
```

**Note:** Cannot cancel completed orders.

## Complete Test Flow

```bash
# 1. Dealer creates order
POST /api/order (Dealer token)
Body: { "listingId": 1, "farmerId": 5, "quantity": 50, "price": 2500 }

# 2. Dealer views their orders
GET /api/order/my-orders (Dealer token)

# 3. Farmer views received orders
GET /api/order/received (Farmer token)

# 4. Farmer confirms order
PUT /api/order/1/status (Farmer token)
Body: { "status": 1 }

# 5. After payment, mark as completed
PUT /api/order/1/status (Farmer token)
Body: { "status": 3 }

# 6. Generate invoice
POST /api/invoice/generate/1 (Farmer token)

# 7. Generate receipt
POST /api/receipt/generate/1 (Farmer token)

# 8. Dealer submits review
POST /api/review (Dealer token)
Body: { "orderId": 1, "farmerId": 5, "rating": 5, "comment": "Great!" }

# 9. View farmer reviews (no auth needed)
GET /api/review/farmer/5
```

## What's Working

✅ Order creation (Dealer only)  
✅ Order viewing (Dealer's orders, Farmer's received orders)  
✅ Order cancellation (Dealer can cancel own orders)  
✅ Order status updates (Farmer/Admin)  
✅ Invoice generation for completed orders  
✅ Receipt generation for completed orders  
✅ Review submission (Dealer for completed orders)  
✅ Review viewing (public access)  
✅ JWT authentication and authorization  
✅ Role-based access control  
✅ Ownership validation  
✅ Business logic validation  

## Project Structure

```
OrderService/
├── Controllers/
│   ├── OrderController.cs            # Order management endpoints
│   ├── InvoiceController.cs          # Invoice endpoints
│   ├── ReceiptController.cs          # Receipt endpoints
│   └── ReviewController.cs           # Review endpoints
├── DTOs/
│   ├── CreateOrderDto.cs
│   ├── OrderResponseDto.cs
│   ├── UpdateOrderStatusDto.cs
│   ├── InvoiceResponseDto.cs
│   ├── ReceiptResponseDto.cs
│   ├── CreateReviewDto.cs
│   └── ReviewResponseDto.cs
├── Data/
│   └── AppDbContext.cs               # EF Core DbContext
├── Models/
│   ├── Order.cs                      # Order entity
│   ├── OrderStatus.cs                # Status enum
│   ├── Invoice.cs                    # Invoice entity
│   ├── Receipt.cs                    # Receipt entity
│   └── Review.cs                     # Review entity
├── Repositories/
│   ├── IOrderRepository.cs
│   ├── OrderRepository.cs
│   ├── IInvoiceRepository.cs
│   ├── InvoiceRepository.cs
│   ├── IReceiptRepository.cs
│   ├── ReceiptRepository.cs
│   ├── IReviewRepository.cs
│   └── ReviewRepository.cs
├── Services/
│   ├── IOrderService.cs
│   ├── OrderService.cs
│   ├── IInvoiceService.cs
│   ├── InvoiceService.cs
│   ├── IReceiptService.cs
│   ├── ReceiptService.cs
│   ├── IReviewService.cs
│   └── ReviewService.cs
├── Migrations/                        # EF Core migrations
├── Program.cs                         # Application startup
├── appsettings.json                  # Configuration
└── README.md                          # Full documentation
```

## Troubleshooting

### JWT Authentication Fails

**Problem:** Getting 401 Unauthorized  
**Solution:**
- Verify JWT Key matches IdentityService exactly
- Check Issuer and Audience values match
- Ensure token format is: `Bearer <token>`
- Verify token hasn't expired

### Cannot Create Order

**Problem:** Authorization fails  
**Solution:**
- Ensure you're logged in as Dealer
- Check JWT token contains correct role
- Verify UserId claim is present in token

### Cannot Generate Invoice/Receipt

**Problem:** "Invoice/Receipt can only be generated for completed orders"  
**Solution:**
- Order status must be Completed (3)
- Update order status first: PUT /api/order/{id}/status with status: 3

### Cannot Submit Review

**Problem:** "Reviews can only be submitted for completed orders"  
**Solution:**
- Order must be completed first
- Dealer must own the order
- Only one review per order allowed
- Rating must be between 1 and 5

### 403 Forbidden on Cancel

**Problem:** Cannot cancel order  
**Solution:**
- Only the dealer who created the order can cancel it
- Cannot cancel completed orders
- Use the same dealer account that created the order

## Sample Data for Testing

### Create Multiple Orders

```json
// Order 1
{ "listingId": 1, "farmerId": 5, "quantity": 50.00, "price": 2500.00 }

// Order 2
{ "listingId": 2, "farmerId": 5, "quantity": 100.00, "price": 2200.00 }

// Order 3
{ "listingId": 3, "farmerId": 6, "quantity": 30.00, "price": 50.00 }
```

### Sample Reviews

```json
// 5-star review
{ "orderId": 1, "farmerId": 5, "rating": 5, "comment": "Excellent quality, highly recommended!" }

// 4-star review
{ "orderId": 2, "farmerId": 5, "rating": 4, "comment": "Good quality, minor delays in delivery." }

// 3-star review
{ "orderId": 3, "farmerId": 6, "rating": 3, "comment": "Average quality, acceptable." }
```

## Integration Points

### IdentityService
- Provides JWT tokens for authentication
- DealerId and FarmerId reference users in IdentityService
- No direct API calls

### CropService
- ListingId references crop listings
- No direct API calls

### PaymentService (Future)
- Will process payments for orders
- Will update order status to Completed
- Will trigger invoice/receipt generation

## Next Steps

Once OrderService is working correctly:
1. Test all endpoints thoroughly
2. Verify JWT authentication across roles
3. Test order lifecycle (create → confirm → complete → review)
4. Test invoice and receipt generation
5. Proceed to build PaymentService
