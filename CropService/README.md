# CropDeal - Crop Service

Crop Service manages crop master data and crop listings for the CropDeal system.

## Technology Stack

- C# / ASP.NET Core 8 Web API
- Entity Framework Core 8
- SQL Server
- JWT Bearer Authentication
- Swagger / OpenAPI

## Project Structure

```
CropService
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

### Crop
Master data for different crop types.

- **Id**: Unique identifier
- **CropName**: Name of the crop (e.g., "Rice", "Wheat")
- **Category**: Category (e.g., "Grain", "Vegetable", "Fruit")
- **Unit**: Measurement unit (e.g., "kg", "quintal", "ton")
- **Description**: Additional information

### CropListing
Individual crop listings created by farmers.

- **Id**: Unique identifier
- **FarmerId**: User ID from JWT (references IdentityService)
- **CropId**: Reference to Crop master data
- **Quantity**: Total quantity available
- **AvailableQuantity**: Currently available quantity
- **ExpectedPrice**: Price per unit
- **Location**: Location of the crop
- **Status**: Available, Sold, or Inactive
- **CreatedAt**: Creation timestamp

## API Endpoints

### Crop Management

#### GET /api/crop
Get all crops (public access).

**Response:**
```json
[
  {
    "id": 1,
    "cropName": "Rice",
    "category": "Grain",
    "unit": "quintal",
    "description": "Basmati rice"
  }
]
```

#### GET /api/crop/{id}
Get crop by ID (public access).

**Response:**
```json
{
  "id": 1,
  "cropName": "Rice",
  "category": "Grain",
  "unit": "quintal",
  "description": "Basmati rice"
}
```

#### POST /api/crop
Create a new crop (Admin only).

**Authorization:** Bearer token with Admin role

**Request Body:**
```json
{
  "cropName": "Rice",
  "category": "Grain",
  "unit": "quintal",
  "description": "Basmati rice"
}
```

**Response:**
- 201 Created
- 400 Bad Request
- 401 Unauthorized

#### PUT /api/crop/{id}
Update existing crop (Admin only).

**Authorization:** Bearer token with Admin role

**Request Body:**
```json
{
  "cropName": "Premium Rice",
  "category": "Grain",
  "unit": "quintal",
  "description": "Premium basmati rice"
}
```

**Response:**
- 200 OK
- 400 Bad Request
- 401 Unauthorized
- 404 Not Found

### Crop Listing Management

#### GET /api/crops/listings
Browse all crop listings (public access).

**Query Parameters:**
- `category` (optional): Filter by crop category
- `cropName` (optional): Filter by crop name
- `location` (optional): Filter by location

**Example:** `/api/crops/listings?category=Grain&location=Punjab`

**Response:**
```json
[
  {
    "id": 1,
    "farmerId": 5,
    "cropId": 1,
    "cropName": "Rice",
    "category": "Grain",
    "unit": "quintal",
    "quantity": 100.00,
    "availableQuantity": 100.00,
    "expectedPrice": 2500.00,
    "location": "Punjab, India",
    "status": 0,
    "createdAt": "2026-10-29T10:00:00Z"
  }
]
```

**Status Values:**
- 0 = Available
- 1 = Sold
- 2 = Inactive

#### GET /api/crops/listings/{id}
Get listing details by ID (public access).

**Response:**
```json
{
  "id": 1,
  "farmerId": 5,
  "cropId": 1,
  "cropName": "Rice",
  "category": "Grain",
  "unit": "quintal",
  "quantity": 100.00,
  "availableQuantity": 100.00,
  "expectedPrice": 2500.00,
  "location": "Punjab, India",
  "status": 0,
  "createdAt": "2026-10-29T10:00:00Z"
}
```

#### POST /api/crops/listings
Create a new crop listing (Farmer only).

**Authorization:** Bearer token with Farmer role

**Request Body:**
```json
{
  "cropId": 1,
  "quantity": 100.00,
  "expectedPrice": 2500.00,
  "location": "Punjab, India"
}
```

**Notes:**
- FarmerId is extracted from JWT token
- AvailableQuantity is automatically set to Quantity
- Status is automatically set to Available
- CreatedAt is automatically set to current UTC time

**Response:**
- 201 Created
- 400 Bad Request (invalid data or crop not found)
- 401 Unauthorized

#### PUT /api/crops/listings/{id}
Update existing listing (Farmer can only update their own listing).

**Authorization:** Bearer token with Farmer role

**Request Body (all fields optional):**
```json
{
  "quantity": 150.00,
  "availableQuantity": 120.00,
  "expectedPrice": 2600.00,
  "location": "Punjab, India",
  "status": 0
}
```

**Response:**
- 200 OK
- 400 Bad Request
- 401 Unauthorized
- 403 Forbidden (trying to update someone else's listing)
- 404 Not Found

## Database Schema

### Crops Table

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key (auto-increment) |
| CropName | nvarchar(100) | Crop name |
| Category | nvarchar(50) | Category |
| Unit | nvarchar(20) | Measurement unit |
| Description | nvarchar(500) | Description |

### CropListings Table

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key (auto-increment) |
| FarmerId | int | User ID from IdentityService |
| CropId | int | Foreign key to Crops |
| Quantity | decimal(18,2) | Total quantity |
| AvailableQuantity | decimal(18,2) | Available quantity |
| ExpectedPrice | decimal(18,2) | Price per unit |
| Location | nvarchar(200) | Location |
| Status | int | 0=Available, 1=Sold, 2=Inactive |
| CreatedAt | datetime2 | Creation timestamp (UTC) |

**Relationships:**
- CropListing → Crop (many-to-one)

## Setup Instructions

### 1. Configure Database Connection

Update `appsettings.json` with your SQL Server connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CropDealCrops;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=True;"
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

**Important:** Use the SAME JWT Key as IdentityService!

### 3. Apply Database Migrations

```bash
dotnet ef database update
```

This creates the `CropDealCrops` database with Crops and CropListings tables.

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
2. First, get a JWT token from IdentityService:
   - Login as Farmer or Admin
   - Copy the token
3. Click **Authorize** button in Swagger UI
4. Enter: `Bearer <your-token>`
5. Test the endpoints

## Testing Flow

### Setup: Create Crop Master Data (Admin)

1. Get Admin JWT token from IdentityService
2. POST `/api/crop` with crop data

```json
{
  "cropName": "Rice",
  "category": "Grain",
  "unit": "quintal",
  "description": "Basmati rice"
}
```

### Create Listing (Farmer)

1. Get Farmer JWT token from IdentityService
2. POST `/api/crops/listings` with listing data

```json
{
  "cropId": 1,
  "quantity": 100.00,
  "expectedPrice": 2500.00,
  "location": "Punjab, India"
}
```

### Browse Listings (Public)

No authentication required:

```
GET /api/crops/listings
GET /api/crops/listings?category=Grain
GET /api/crops/listings/{id}
```

### Update Listing (Farmer)

Farmer can only update their own listings:

```
PUT /api/crops/listings/{id}
```

## Authorization Rules

| Endpoint | Access |
|----------|--------|
| GET /api/crop | Public |
| GET /api/crop/{id} | Public |
| POST /api/crop | Admin only |
| PUT /api/crop/{id} | Admin only |
| GET /api/crops/listings | Public |
| GET /api/crops/listings/{id} | Public |
| POST /api/crops/listings | Farmer only |
| PUT /api/crops/listings/{id} | Farmer (own listings only) |

## Security Features

- **JWT Authentication**: Same configuration as IdentityService
- **Role-Based Authorization**: Admin, Farmer roles enforced
- **Ownership Validation**: Farmers can only modify their own listings
- **Public Browsing**: Listings are browsable without authentication
- **FarmerId from JWT**: Cannot be spoofed by client

## Integration with IdentityService

CropService depends on IdentityService for:
- JWT token generation
- User authentication
- Role information

**Note:** CropService does NOT call IdentityService APIs. It validates JWT tokens independently using the shared JWT Key.

The `FarmerId` field in CropListing references a user in IdentityService, but this is a logical reference only (no foreign key constraint).

## Common Issues

### Database Connection Failed
- Ensure SQL Server is running
- Verify connection string in `appsettings.json`
- Check firewall settings

### JWT Token Invalid
- Verify JWT Key matches IdentityService
- Ensure Issuer and Audience match
- Check token format: `Bearer <token>`
- Verify token hasn't expired

### Unauthorized Errors
- Ensure you're logged in as the correct role
- Admin for crop management
- Farmer for listing operations
- Check JWT token is included in request

### Cannot Create Listing
- Verify the CropId exists (create crops first as Admin)
- Ensure you're authenticated as Farmer
- Check all required fields are provided

## Development Notes

- CropService uses its own database (CropDealCrops)
- FarmerId is extracted from JWT claims, not from request body
- Farmers can only update their own listings
- Public endpoints allow browsing without authentication
- Decimal precision is 18,2 for all monetary and quantity values
- All timestamps use UTC

## Next Steps

After CropService is working:
1. Create OrderService
2. Create PaymentService
3. Integrate all services
