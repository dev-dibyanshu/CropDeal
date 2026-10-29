# CropDeal - Identity Service

Identity Service handles user authentication and authorization for the CropDeal system.

## Technology Stack

- C# / ASP.NET Core 8 Web API
- Entity Framework Core 8
- SQL Server
- JWT Bearer Authentication
- BCrypt for password hashing
- Swagger / OpenAPI

## Project Structure

```
IdentityService
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

## User Roles

- **Farmer**: Can register and use the platform
- **Dealer**: Can register and use the platform
- **Admin**: Cannot be self-registered (system role)

## API Endpoints

### POST /api/auth/register
Register a new user (Farmer or Dealer only).

**Request Body:**
```json
{
  "fullName": "John Doe",
  "email": "john@example.com",
  "mobileNumber": "1234567890",
  "password": "SecurePass123",
  "role": 0
}
```

**Roles:**
- 0 = Farmer
- 1 = Dealer

**Response:**
- 201 Created - Registration successful
- 400 Bad Request - Invalid data or attempting to register as Admin
- 409 Conflict - Email already exists

### POST /api/auth/login
Login with email and password.

**Request Body:**
```json
{
  "email": "john@example.com",
  "password": "SecurePass123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 1,
  "fullName": "John Doe",
  "email": "john@example.com",
  "role": 0
}
```

- 200 OK - Login successful
- 401 Unauthorized - Invalid credentials

### GET /api/auth/me
Get current authenticated user information (requires JWT token).

**Headers:**
```
Authorization: Bearer <your-jwt-token>
```

**Response:**
```json
{
  "userId": "1",
  "email": "john@example.com",
  "role": "Farmer",
  "message": "Authenticated user information"
}
```

- 200 OK - User info returned
- 401 Unauthorized - Invalid or missing token

## Setup Instructions

### 1. Configure Database Connection

Update `appsettings.json` with your SQL Server connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CropDealIdentity;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=True;"
}
```

### 2. Apply Database Migrations

```bash
dotnet ef database update
```

This will create the `CropDealIdentity` database with the `Users` table.

### 3. Configure JWT Settings (Optional)

Update JWT settings in `appsettings.json` if needed:

```json
"Jwt": {
  "Key": "your-very-secure-secret-key-minimum-32-characters-long",
  "Issuer": "CropDealIdentityService",
  "Audience": "CropDeal",
  "ExpiryMinutes": 60
}
```

**Important:** Change the JWT Key in production!

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
2. Test the `/api/auth/register` endpoint to create a user
3. Test the `/api/auth/login` endpoint to get a JWT token
4. Click the **Authorize** button at the top of Swagger UI
5. Enter: `Bearer <your-token-here>` (including the word "Bearer")
6. Test the `/api/auth/me` endpoint to verify authentication

## Database Schema

### Users Table

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key (auto-increment) |
| FullName | nvarchar(100) | User's full name |
| Email | nvarchar(100) | Unique email address |
| MobileNumber | nvarchar(20) | Mobile number |
| PasswordHash | nvarchar(max) | BCrypt hashed password |
| Role | int | User role (0=Farmer, 1=Dealer, 2=Admin) |
| CreatedAt | datetime2 | Account creation timestamp |
| IsActive | bit | Account status flag |

## Security Features

- **Password Hashing**: BCrypt with automatic salt generation
- **JWT Authentication**: Stateless token-based authentication
- **Role-Based Access**: Admin role cannot be self-registered
- **Email Uniqueness**: Enforced at database level
- **HTTPS**: Enabled by default

## Development Notes

- Passwords are never stored in plain text
- Admin users must be created manually in the database
- JWT tokens expire after 60 minutes (configurable)
- All timestamps use UTC
- Email addresses are case-insensitive for login

## Common Issues

### Database Connection Failed
- Ensure SQL Server is running
- Verify connection string in `appsettings.json`
- Check firewall settings

### JWT Token Invalid
- Verify the token format: `Bearer <token>`
- Check token expiration (default 60 minutes)
- Ensure JWT Key matches between token generation and validation

### Migration Errors
- Delete Migrations folder and recreate: `dotnet ef migrations add InitialCreate`
- Drop database and recreate: `dotnet ef database drop` then `dotnet ef database update`

## Next Steps

After IdentityService is working:
1. Create CropService
2. Create OrderService
3. Create PaymentService
4. Integrate services with API Gateway
