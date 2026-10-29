# Quick Start Guide - CropService

## Prerequisites

- .NET 8 SDK installed
- SQL Server running
- IdentityService running (to get JWT tokens)

## Steps to Run

### 1. Update Database Connection String

Edit `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CropDealCrops;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
}
```

### 2. Ensure JWT Settings Match IdentityService

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
cd CropService
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

### Step 1: Get JWT Tokens from IdentityService

First, you need tokens from IdentityService:

#### Register an Admin (manually in database)
For testing, insert an Admin user directly:

```sql
INSERT INTO CropDealIdentity.dbo.Users (FullName, Email, MobileNumber, PasswordHash, Role, CreatedAt, IsActive)
VALUES ('Admin User', 'admin@cropdeal.com', '0000000000', '$2a$11$...', 2, GETUTCDATE(), 1);
```

Or register Farmer users via IdentityService API.

#### Login to Get Tokens

From IdentityService Swagger:

```json
// Login as Admin
POST /api/auth/login
{
  "email": "admin@cropdeal.com",
  "password": "AdminPass123"
}

// Login as Farmer
POST /api/auth/login
{
  "email": "farmer@example.com",
  "password": "FarmerPass123"
}
```

Copy the tokens from responses.

### Step 2: Authorize in CropService Swagger

1. Click **Authorize** button at the top
2. Enter: `Bearer <your-admin-token>`
3. Click **Authorize**

### Step 3: Create Crop Master Data (Admin)

Now test creating crops:

```json
POST /api/crop
{
  "cropName": "Rice",
  "category": "Grain",
  "unit": "quintal",
  "description": "Basmati rice variety"
}
```

Create a few more:

```json
{
  "cropName": "Wheat",
  "category": "Grain",
  "unit": "quintal",
  "description": "Premium wheat"
}

{
  "cropName": "Tomato",
  "category": "Vegetable",
  "unit": "kg",
  "description": "Fresh tomatoes"
}
```

### Step 4: Browse Crops (Public)

These endpoints work without authentication:

```
GET /api/crop
GET /api/crop/1
```

### Step 5: Create Listing (Farmer)

1. Click **Authorize** again
2. Enter: `Bearer <your-farmer-token>`
3. Click **Authorize**

Now create a listing:

```json
POST /api/crops/listings
{
  "cropId": 1,
  "quantity": 100.50,
  "expectedPrice": 2500.00,
  "location": "Punjab, India"
}
```

Create more listings:

```json
{
  "cropId": 2,
  "quantity": 200.00,
  "expectedPrice": 2200.00,
  "location": "Haryana, India"
}

{
  "cropId": 3,
  "quantity": 50.00,
  "expectedPrice": 30.00,
  "location": "Maharashtra, India"
}
```

### Step 6: Browse Listings (Public)

No authentication needed:

```
GET /api/crops/listings
GET /api/crops/listings?category=Grain
GET /api/crops/listings?location=Punjab
GET /api/crops/listings/1
```

### Step 7: Update Listing (Farmer)

Farmers can update their own listings:

```json
PUT /api/crops/listings/1
{
  "availableQuantity": 80.00,
  "expectedPrice": 2600.00
}
```

Mark as sold:

```json
PUT /api/crops/listings/1
{
  "status": 1
}
```

**Status Values:**
- 0 = Available
- 1 = Sold
- 2 = Inactive

### Step 8: Try Unauthorized Update

1. Get a DIFFERENT farmer's token
2. Try to update the first farmer's listing
3. You should get a 403 Forbidden response

## Complete Test Flow

```bash
# 1. Admin creates crops
POST /api/crop (Admin token)
Body: { "cropName": "Rice", "category": "Grain", "unit": "quintal", "description": "..." }

# 2. View crops (no auth needed)
GET /api/crop

# 3. Farmer creates listing
POST /api/crops/listings (Farmer token)
Body: { "cropId": 1, "quantity": 100, "expectedPrice": 2500, "location": "Punjab" }

# 4. Browse listings (no auth needed)
GET /api/crops/listings
GET /api/crops/listings?category=Grain

# 5. View listing details (no auth needed)
GET /api/crops/listings/1

# 6. Update own listing (Farmer token - must be same farmer)
PUT /api/crops/listings/1
Body: { "availableQuantity": 80 }

# 7. Try to update another farmer's listing (should fail with 403)
PUT /api/crops/listings/2 (different farmer's listing)
```

## What's Working

✅ Crop master data management (Admin only)  
✅ Browse crops (public)  
✅ Create crop listings (Farmer only)  
✅ Browse listings with filtering (public)  
✅ Update own listings (Farmer only)  
✅ Ownership validation  
✅ JWT authentication and authorization  
✅ Role-based access control  
✅ Swagger UI with Bearer token support  

## Project Structure

```
CropService/
├── Controllers/
│   ├── CropController.cs             # Crop management endpoints
│   └── CropListingController.cs      # Listing management endpoints
├── DTOs/
│   ├── CreateCropDto.cs
│   ├── UpdateCropDto.cs
│   ├── CropResponseDto.cs
│   ├── CreateCropListingDto.cs
│   ├── UpdateCropListingDto.cs
│   └── CropListingResponseDto.cs
├── Data/
│   └── AppDbContext.cs               # EF Core DbContext
├── Models/
│   ├── Crop.cs                       # Crop entity
│   ├── CropListing.cs                # Listing entity
│   └── CropListingStatus.cs          # Status enum
├── Repositories/
│   ├── ICropRepository.cs
│   ├── CropRepository.cs
│   ├── ICropListingRepository.cs
│   └── CropListingRepository.cs
├── Services/
│   ├── ICropService.cs
│   ├── CropService.cs
│   ├── ICropListingService.cs
│   └── CropListingService.cs
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

### Cannot Create Listing - "Crop not found"

**Problem:** CropId doesn't exist  
**Solution:**
- Create crops first as Admin
- Use GET /api/crop to verify crop IDs
- Use the correct crop ID in your listing

### 403 Forbidden on Update

**Problem:** Trying to update another farmer's listing  
**Solution:**
- Each farmer can only update their own listings
- FarmerId is extracted from JWT token
- Use the same farmer account that created the listing

### Database Connection Issues

**Problem:** Cannot connect to SQL Server  
**Solution:**
- Verify SQL Server is running
- Check connection string credentials
- Ensure database exists (run migrations)

## Sample Data for Testing

### Crops (Admin)
```json
// Rice
{ "cropName": "Basmati Rice", "category": "Grain", "unit": "quintal", "description": "Premium basmati" }

// Wheat
{ "cropName": "Durum Wheat", "category": "Grain", "unit": "quintal", "description": "High protein wheat" }

// Vegetables
{ "cropName": "Tomato", "category": "Vegetable", "unit": "kg", "description": "Fresh tomatoes" }
{ "cropName": "Potato", "category": "Vegetable", "unit": "kg", "description": "Fresh potatoes" }
{ "cropName": "Onion", "category": "Vegetable", "unit": "kg", "description": "Red onions" }
```

### Listings (Farmer)
```json
// Rice listing
{ "cropId": 1, "quantity": 100.00, "expectedPrice": 2500.00, "location": "Punjab, India" }

// Wheat listing
{ "cropId": 2, "quantity": 150.00, "expectedPrice": 2200.00, "location": "Haryana, India" }

// Vegetable listings
{ "cropId": 3, "quantity": 500.00, "expectedPrice": 30.00, "location": "Maharashtra, India" }
{ "cropId": 4, "quantity": 800.00, "expectedPrice": 25.00, "location": "Uttar Pradesh, India" }
```

## Next Steps

Once CropService is working correctly:
1. Test all endpoints thoroughly
2. Verify JWT authentication across roles
3. Test ownership validation for listings
4. Check filtering on browse endpoints
5. Proceed to build OrderService
