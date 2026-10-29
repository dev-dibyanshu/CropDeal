# Quick Start Guide - IdentityService

## Prerequisites

- .NET 8 SDK installed
- SQL Server running (local or remote)

## Steps to Run

### 1. Update Database Connection String

Edit `appsettings.json` and update the connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CropDealIdentity;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
}
```

### 2. Apply Migrations

```bash
cd IdentityService
dotnet ef database update
```

This creates the database and tables.

### 3. Run the Application

```bash
dotnet run
```

### 4. Open Swagger UI

Open your browser and navigate to:
```
https://localhost:<port>/swagger
```

The port number will be shown in the console output.

### 5. Test the API

#### Register a User

1. Click on **POST /api/auth/register**
2. Click **Try it out**
3. Use this JSON (modify as needed):

```json
{
  "fullName": "John Farmer",
  "email": "farmer@example.com",
  "mobileNumber": "9876543210",
  "password": "Password123",
  "role": 0
}
```

**Note:** Role values:
- 0 = Farmer
- 1 = Dealer
- 2 = Admin (cannot be registered via API)

4. Click **Execute**
5. You should get a **201 Created** response

#### Login

1. Click on **POST /api/auth/login**
2. Click **Try it out**
3. Use this JSON:

```json
{
  "email": "farmer@example.com",
  "password": "Password123"
}
```

4. Click **Execute**
5. Copy the **token** from the response

#### Test Protected Endpoint

1. Click the **Authorize** button at the top of the Swagger page
2. In the value field, enter: `Bearer <paste-your-token-here>`
   - Example: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
3. Click **Authorize**
4. Now click on **GET /api/auth/me**
5. Click **Try it out**
6. Click **Execute**
7. You should see your user information in the response

## Troubleshooting

### Database Connection Issues

If you get a connection error:
- Make sure SQL Server is running
- Verify your connection string credentials
- Check if the server allows remote connections (if not using localhost)

### Migration Issues

If migrations fail:
```bash
# Remove existing migrations
rm -rf Migrations/

# Recreate migrations
dotnet ef migrations add InitialCreate

# Apply to database
dotnet ef database update
```

### JWT Token Issues

If authentication fails:
- Make sure you included the word "Bearer" before the token
- Check if the token has expired (default: 60 minutes)
- Verify the JWT Key in appsettings.json is at least 32 characters

## Sample Test Flow

```bash
# 1. Register a Farmer
POST /api/auth/register
{
  "fullName": "Alice Farmer",
  "email": "alice@farm.com",
  "mobileNumber": "1111111111",
  "password": "FarmerPass123",
  "role": 0
}

# 2. Register a Dealer
POST /api/auth/register
{
  "fullName": "Bob Dealer",
  "email": "bob@dealer.com",
  "mobileNumber": "2222222222",
  "password": "DealerPass123",
  "role": 1
}

# 3. Login as Farmer
POST /api/auth/login
{
  "email": "alice@farm.com",
  "password": "FarmerPass123"
}
# Copy the token from response

# 4. Use token to access protected endpoint
GET /api/auth/me
Authorization: Bearer <your-token>
```

## What's Working

✅ User registration (Farmer and Dealer only)  
✅ Email uniqueness validation  
✅ Password hashing with BCrypt  
✅ User login with credential validation  
✅ JWT token generation  
✅ Protected endpoint with JWT authentication  
✅ Swagger UI with JWT Bearer authorization  
✅ SQL Server with Entity Framework Core  
✅ Migration-based database schema  

## Project Structure

```
IdentityService/
├── Controllers/
│   └── AuthController.cs          # API endpoints
├── DTOs/
│   ├── RegisterDto.cs             # Registration request
│   ├── LoginDto.cs                # Login request
│   └── LoginResponseDto.cs        # Login response
├── Data/
│   └── AppDbContext.cs            # EF Core DbContext
├── Models/
│   ├── User.cs                    # User entity
│   └── UserRole.cs                # Role enum
├── Repositories/
│   ├── IUserRepository.cs         # Repository interface
│   └── UserRepository.cs          # Repository implementation
├── Services/
│   ├── IAuthService.cs            # Service interface
│   └── AuthService.cs             # Auth service (registration, login, JWT)
├── Migrations/                     # EF Core migrations
├── Program.cs                      # Application startup
├── appsettings.json               # Configuration
└── README.md                       # Full documentation
```

## Next Steps

Once IdentityService is working correctly, you can:
1. Test all endpoints thoroughly
2. Verify JWT authentication works across requests
3. Check database records in SQL Server
4. Proceed to build CropService next
