# CropDeal Backend - Microservices Architecture

CropDeal is a platform connecting farmers with dealers for crop trading.

## Architecture Overview

CropDeal backend consists of four independent microservices:

1. **IdentityService** ✅ - User authentication and authorization
2. **CropService** ✅ - Crop management and listings
3. **OrderService** 🔜 - Order processing
4. **PaymentService** 🔜 - Payment handling

## Current Status

### ✅ IdentityService (Complete)
- User registration (Farmer, Dealer)
- User login with JWT
- Role-based authentication
- Admin, Farmer, Dealer roles
- Database: `CropDealIdentity`
- Port: Configured in launchSettings.json

### ✅ CropService (Complete)
- Crop master data management
- Crop listing by farmers
- Public browsing with filters
- Ownership validation
- Database: `CropDealCrops`
- Port: Configured in launchSettings.json

### 🔜 OrderService (Planned)
- Order creation from listings
- Order status management
- Order history
- Database: `CropDealOrders`

### ✅ PaymentService (Complete)
- Payment processing
- Transaction management
- Payment history
- Refund functionality
- Simulated payment gateway
- Database: `CropDealPayments`
- Port: Configured in launchSettings.json

## Technology Stack

- **Language**: C#
- **Framework**: ASP.NET Core 8 Web API
- **ORM**: Entity Framework Core 8
- **Database**: SQL Server
- **Authentication**: JWT Bearer
- **API Documentation**: Swagger/OpenAPI

## Project Structure

```
CropDeal/
│
├── IdentityService/
│   ├── Controllers/
│   ├── DTOs/
│   ├── Data/
│   ├── Migrations/
│   ├── Models/
│   ├── Repositories/
│   ├── Services/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── README.md
│   └── QUICKSTART.md
│
├── CropService/
│   ├── Controllers/
│   ├── DTOs/
│   ├── Data/
│   ├── Migrations/
│   ├── Models/
│   ├── Repositories/
│   ├── Services/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── README.md
│   └── QUICKSTART.md
│
├── OrderService/
│   ├── Controllers/
│   ├── DTOs/
│   ├── Data/
│   ├── Migrations/
│   ├── Models/
│   ├── Repositories/
│   ├── Services/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── README.md
│   └── QUICKSTART.md
│
├── PaymentService/
│   ├── Controllers/
│   ├── DTOs/
│   ├── Data/
│   ├── Migrations/
│   ├── Models/
│   ├── Repositories/
│   ├── Services/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── README.md
│   └── QUICKSTART.md
│
└── README.md            (This file)
```

## Architecture Principles

### Microservice Design
- Each service has its own database
- Services are independently deployable
- No direct service-to-service API calls (in current implementation)
- Shared JWT authentication configuration

### Simple Layered Architecture
Each microservice follows:
```
Controller → Service → Repository → DbContext → Database
```

### No Over-Engineering
We deliberately avoid:
- Clean Architecture
- CQRS
- MediatR
- AutoMapper
- Generic repositories
- Unnecessary abstractions

## Databases

| Service | Database Name | Purpose |
|---------|---------------|---------|
| IdentityService | CropDealIdentity | Users, roles, authentication |
| CropService | CropDealCrops | Crops, listings |
| OrderService | CropDealOrders | Orders, invoices, receipts, reviews |
| PaymentService | CropDealPayments | Payments, transactions, refunds |

## JWT Configuration

All services share the same JWT configuration for authentication:

```json
"Jwt": {
  "Key": "your-very-secure-secret-key-minimum-32-characters-long",
  "Issuer": "CropDealIdentityService",
  "Audience": "CropDeal"
}
```

**Important:** The JWT Key must be identical across all services!

## User Roles

### Farmer
- Register and login
- Create crop listings
- Update own listings
- Browse all listings

### Dealer
- Register and login
- Browse listings
- Place orders (OrderService)
- View order history
- Cancel orders before completion
- Submit reviews after completion
- Make payments (PaymentService)

### Admin
- Manage crop master data
- System administration

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server
- Visual Studio Code or Visual Studio

### Setup Steps

1. **Clone the repository**
   ```bash
   cd CropDeal
   ```

2. **Configure SQL Server connection strings**
   
   Update `appsettings.json` in each service:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=<DatabaseName>;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
   }
   ```

3. **Configure JWT settings**
   
   Ensure the JWT Key is SAME across all services in `appsettings.json`:
   ```json
   "Jwt": {
     "Key": "your-very-secure-secret-key-minimum-32-characters-long",
     "Issuer": "CropDealIdentityService",
     "Audience": "CropDeal"
   }
   ```

4. **Apply migrations for each service**
   
   ```bash
   # IdentityService
   cd IdentityService
   dotnet ef database update
   
   # CropService
   cd ../CropService
   dotnet ef database update
   
   # OrderService
   cd ../OrderService
   dotnet ef database update
   
   # PaymentService
   cd ../PaymentService
   dotnet ef database update
   ```

5. **Run services**
   
   Open separate terminals for each service:
   
   ```bash
   # Terminal 1 - IdentityService
   cd IdentityService
   dotnet run
   
   # Terminal 2 - CropService
   cd CropService
   dotnet run
   
   # Terminal 3 - OrderService
   cd OrderService
   dotnet run
   
   # Terminal 4 - PaymentService
   cd PaymentService
   dotnet run
   ```

6. **Access Swagger UI**
   
   - IdentityService: https://localhost:7xxx/swagger
   - CropService: https://localhost:7yyy/swagger
   - OrderService: https://localhost:7zzz/swagger
   - PaymentService: https://localhost:7www/swagger
   
   (Check console output for exact ports)

## Testing Workflow

### 1. Register Users (IdentityService)

```bash
# Register a Farmer
POST https://localhost:7xxx/api/auth/register
{
  "fullName": "John Farmer",
  "email": "farmer@example.com",
  "mobileNumber": "1234567890",
  "password": "Password123",
  "role": 0
}

# Register a Dealer
POST https://localhost:7xxx/api/auth/register
{
  "fullName": "Bob Dealer",
  "email": "dealer@example.com",
  "mobileNumber": "0987654321",
  "password": "Password123",
  "role": 1
}
```

### 2. Login and Get JWT Token

```bash
POST https://localhost:7xxx/api/auth/login
{
  "email": "farmer@example.com",
  "password": "Password123"
}

# Copy the token from response
```

### 3. Create Crops (Admin - CropService)

First, create an Admin user manually in the database, then:

```bash
POST https://localhost:7yyy/api/crop
Authorization: Bearer <admin-token>
{
  "cropName": "Rice",
  "category": "Grain",
  "unit": "quintal",
  "description": "Basmati rice"
}
```

### 4. Create Listing (Farmer - CropService)

```bash
POST https://localhost:7yyy/api/crops/listings
Authorization: Bearer <farmer-token>
{
  "cropId": 1,
  "quantity": 100.00,
  "expectedPrice": 2500.00,
  "location": "Punjab, India"
}
```

### 5. Create Listing (Farmer - CropService)

```bash
POST https://localhost:7yyy/api/crops/listings
Authorization: Bearer <farmer-token>
{
  "cropId": 1,
  "quantity": 100.00,
  "expectedPrice": 2500.00,
  "location": "Punjab, India"
}
```

### 6. Browse Listings (Public - CropService)

```bash
# No authentication required
GET https://localhost:7yyy/api/crops/listings
GET https://localhost:7yyy/api/crops/listings?category=Grain
```

### 7. Place Order (Dealer - OrderService)

```bash
POST https://localhost:7zzz/api/order
Authorization: Bearer <dealer-token>
{
  "listingId": 1,
  "farmerId": 5,
  "quantity": 50.00,
  "price": 2500.00
}
```

### 8. Farmer Confirms Order (OrderService)

```bash
PUT https://localhost:7zzz/api/order/1/status
Authorization: Bearer <farmer-token>
{
  "status": 1
}
```

### 9. Complete Order and Generate Documents (OrderService)

```bash
# Mark as completed
PUT https://localhost:7zzz/api/order/1/status
{
  "status": 3
}

# Generate invoice
POST https://localhost:7zzz/api/invoice/generate/1

# Generate receipt
POST https://localhost:7zzz/api/receipt/generate/1
```

### 10. Submit Review (Dealer - OrderService)

```bash
POST https://localhost:7zzz/api/review
Authorization: Bearer <dealer-token>
{
  "orderId": 1,
  "farmerId": 5,
  "rating": 5,
  "comment": "Excellent quality!"
}
```

### 11. Make Payment (Dealer - PaymentService)

```bash
POST https://localhost:7www/api/payment
Authorization: Bearer <dealer-token>
{
  "orderId": 1,
  "farmerId": 5,
  "amount": 125000.00,
  "paymentMethod": "CreditCard"
}
```

### 12. View Payment History (PaymentService)

```bash
# Dealer's payments
GET https://localhost:7www/api/payment/history
Authorization: Bearer <dealer-token>

# Farmer's received payments
GET https://localhost:7www/api/payment/history
Authorization: Bearer <farmer-token>
```

## Service Communication

### Current Implementation (Phase 1)
Services are independent and do not communicate with each other. User identification across services is handled through:
- JWT tokens containing UserId and Role
- Services store userId as integer references
- No direct API calls between services

### Future Enhancements (Phase 2+)
- Message queue (RabbitMQ) for async communication
- API Gateway for unified entry point
- Service discovery
- Circuit breakers
- Distributed tracing

## API Documentation

Each service has its own Swagger UI:

- **IdentityService**: https://localhost:7xxx/swagger
  - User registration and login
  - JWT token generation
  
- **CropService**: https://localhost:7yyy/swagger
  - Crop management (Admin)
  - Listing management (Farmer)
  - Public browsing

- **PaymentService**: https://localhost:7www/swagger
  - Payment processing (Dealer)
  - Payment history (Dealer/Farmer)
  - Refund management (Admin)

## Security

### Authentication
- JWT Bearer token authentication
- Tokens issued by IdentityService
- Tokens validated by all services using shared key

### Authorization
- Role-based access control
- Admin, Farmer, Dealer roles
- Ownership validation for resources

### Data Protection
- Password hashing with BCrypt
- HTTPS enabled by default
- No sensitive data in responses
- Input validation on all endpoints

## Development Guidelines

### Code Style
- Simple, readable C# code
- Follow SOLID principles without over-engineering
- Use async/await properly
- Dependency injection for services
- Repository pattern for data access
- DTOs for API contracts

### Database Migrations
- Use EF Core migrations
- One migration per logical change
- Never use Database.EnsureCreated()
- Always test migrations

### API Design
- RESTful conventions
- Proper HTTP status codes
- Consistent response formats
- Clear error messages

## Troubleshooting

### JWT Authentication Issues
**Problem:** 401 Unauthorized across services

**Solution:**
- Verify JWT Key is IDENTICAL in all services
- Check Issuer and Audience match
- Ensure token format: `Bearer <token>`
- Verify token hasn't expired

### Database Connection Issues
**Problem:** Cannot connect to SQL Server

**Solution:**
- Verify SQL Server is running
- Check connection strings
- Ensure databases exist (run migrations)
- Check firewall settings

### Port Conflicts
**Problem:** Port already in use

**Solution:**
- Check launchSettings.json in Properties folder
- Modify ports if needed
- Ensure no other services using same ports

## Documentation

Each service has detailed documentation:

- `README.md` - Complete documentation
- `QUICKSTART.md` - Quick start guide

## Next Steps

### Backend Complete! 🎉

All four microservices are now operational:
- ✅ IdentityService
- ✅ CropService
- ✅ OrderService
- ✅ PaymentService

### Recommended Next Steps

1. **Testing & Integration**
   - Test complete end-to-end workflow
   - Verify JWT authentication across all services
   - Test role-based authorization
   - Validate business logic and data flow

2. **API Gateway** (Optional)
   - Implement API Gateway for unified entry point
   - Route requests to appropriate microservices
   - Handle cross-cutting concerns

3. **Message Queue** (Optional)
   - Add RabbitMQ or similar for async communication
   - Implement event-driven architecture
   - Handle service-to-service notifications

4. **Frontend Development**
   - Single entry point
   - Request routing
   - Authentication gateway

5. **Add Message Queue**
   - Async communication
   - Event-driven architecture
   - RabbitMQ or similar

## Contributing

When adding new features:
1. Create feature branch
2. Follow existing code patterns
3. Add proper validation
4. Update documentation
5. Test thoroughly

## License

[Your License Here]

## Contact

[Your Contact Information]
