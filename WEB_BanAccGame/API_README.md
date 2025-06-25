# Products API Documentation

This API provides endpoints for managing game account products in the WEB_BanAccGame application.

## Base URL
```
/api/ProductsApi
```

## Authentication
- **GET** endpoints: No authentication required
- **POST, PUT, DELETE** endpoints: Require Admin role authentication

## Endpoints

### 1. Get All Products
**GET** `/api/ProductsApi`

Retrieves a paginated list of products with optional filtering and sorting.

#### Query Parameters
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| page | int | 1 | Page number for pagination |
| pageSize | int | 10 | Number of items per page |
| search | string | null | Search term for title/description |
| categoryId | int | null | Filter by category ID |
| isAvailable | bool | null | Filter by availability status |
| sortBy | string | "CreatedDate" | Sort field (title, price, createddate) |
| sortOrder | string | "desc" | Sort order (asc, desc) |

#### Example Request
```
GET /api/ProductsApi?page=1&pageSize=5&search=lien%20quan&categoryId=1&isAvailable=true&sortBy=price&sortOrder=asc
```

#### Example Response
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "title": "Account Liên Quân VIP - Rank Cao Thủ",
      "description": "Account Liên Quân Mobile rank Cao Thủ với đầy đủ tướng và skin hiếm.",
      "price": 2500000,
      "server": "Server 1",
      "level": 30,
      "imageUrl": "https://via.placeholder.com/400x300/FF6B6B/ffffff?text=Lien+Quan+VIP",
      "isAvailable": true,
      "createdDate": "2024-06-18T15:30:00",
      "categoryId": 1,
      "categoryName": "MMORPG",
      "accountUsername": "lqmvip123",
      "accountPassword": "password123"
    }
  ],
  "message": "Products retrieved successfully",
  "totalCount": 1,
  "page": 1,
  "pageSize": 5,
  "totalPages": 1
}
```

### 2. Get Single Product
**GET** `/api/ProductsApi/{id}`

Retrieves a specific product by its ID.

#### Path Parameters
| Parameter | Type | Description |
|-----------|------|-------------|
| id | int | Product ID |

#### Example Request
```
GET /api/ProductsApi/1
```

#### Example Response
```json
{
  "success": true,
  "data": {
    "id": 1,
    "title": "Account Liên Quân VIP - Rank Cao Thủ",
    "description": "Account Liên Quân Mobile rank Cao Thủ với đầy đủ tướng và skin hiếm.",
    "price": 2500000,
    "server": "Server 1",
    "level": 30,
    "imageUrl": "https://via.placeholder.com/400x300/FF6B6B/ffffff?text=Lien+Quan+VIP",
    "isAvailable": true,
    "createdDate": "2024-06-18T15:30:00",
    "categoryId": 1,
    "categoryName": "MMORPG",
    "accountUsername": "lqmvip123",
    "accountPassword": "password123"
  },
  "message": "Product retrieved successfully"
}
```

### 3. Create Product
**POST** `/api/ProductsApi` *(Admin Only)*

Creates a new product.

#### Request Body
```json
{
  "title": "New Game Account",
  "description": "Description of the game account",
  "price": 1500000,
  "server": "Server 1",
  "level": 25,
  "imageUrl": "https://example.com/image.jpg",
  "isAvailable": true,
  "categoryId": 1,
  "accountUsername": "username123",
  "accountPassword": "password123"
}
```

#### Field Validation
| Field | Required | Type | Max Length | Notes |
|-------|----------|------|------------|-------|
| title | Yes | string | 200 | Product title |
| description | No | string | 2000 | Product description |
| price | Yes | decimal | - | Must be between 0.01 and 999,999,999 |
| server | No | string | 100 | Game server |
| level | No | int | - | Account level |
| imageUrl | No | string | 500 | Image URL |
| isAvailable | No | bool | - | Default: true |
| categoryId | Yes | int | - | Must exist in database |
| accountUsername | No | string | 100 | Game account username |
| accountPassword | No | string | 100 | Game account password |

#### Example Response
```json
{
  "success": true,
  "data": {
    "id": 5,
    "title": "New Game Account",
    "description": "Description of the game account",
    "price": 1500000,
    "server": "Server 1",
    "level": 25,
    "imageUrl": "https://example.com/image.jpg",
    "isAvailable": true,
    "createdDate": "2024-06-18T16:00:00",
    "categoryId": 1,
    "categoryName": "MMORPG",
    "accountUsername": "username123",
    "accountPassword": "password123"
  },
  "message": "Product created successfully"
}
```

### 4. Update Product
**PUT** `/api/ProductsApi/{id}` *(Admin Only)*

Updates an existing product.

#### Path Parameters
| Parameter | Type | Description |
|-----------|------|-------------|
| id | int | Product ID to update |

#### Request Body
Same as Create Product request body with same validation rules.

#### Example Response
```json
{
  "success": true,
  "data": {
    "id": 1,
    "title": "Updated Game Account",
    "description": "Updated description",
    "price": 2000000,
    "server": "Server 2",
    "level": 35,
    "imageUrl": "https://example.com/updated-image.jpg",
    "isAvailable": true,
    "createdDate": "2024-06-18T15:30:00",
    "categoryId": 2,
    "categoryName": "FPS",
    "accountUsername": "updated_username",
    "accountPassword": "updated_password"
  },
  "message": "Product updated successfully"
}
```

### 5. Delete Product
**DELETE** `/api/ProductsApi/{id}` *(Admin Only)*

Deletes a product. Cannot delete products that are referenced in existing orders.

#### Path Parameters
| Parameter | Type | Description |
|-----------|------|-------------|
| id | int | Product ID to delete |

#### Example Response
```json
{
  "success": true,
  "message": "Product deleted successfully"
}
```

#### Error Response (if referenced in orders)
```json
{
  "success": false,
  "message": "Cannot delete product as it is referenced in existing orders"
}
```

### 6. Get Categories
**GET** `/api/ProductsApi/categories`

Retrieves all available product categories.

#### Example Response
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "MMORPG",
      "description": "Massively Multiplayer Online Role-Playing Game",
      "iconUrl": "🎮"
    },
    {
      "id": 2,
      "name": "FPS",
      "description": "First-Person Shooter",
      "iconUrl": "🔫"
    },
    {
      "id": 3,
      "name": "MOBA",
      "description": "Multiplayer Online Battle Arena",
      "iconUrl": "⚔️"
    },
    {
      "id": 4,
      "name": "Battle Royale",
      "description": "Last Man Standing Games",
      "iconUrl": "🏆"
    },
    {
      "id": 5,
      "name": "Survival",
      "description": "Survival Games",
      "iconUrl": "🏕️"
    }
  ],
  "message": "Categories retrieved successfully"
}
```

## Error Responses

### Common Error Response Format
```json
{
  "success": false,
  "message": "Error description",
  "errors": ["Detailed error 1", "Detailed error 2"]
}
```

### HTTP Status Codes
- **200 OK**: Successful GET, PUT requests
- **201 Created**: Successful POST requests
- **400 Bad Request**: Invalid input data or validation errors
- **401 Unauthorized**: Authentication required
- **403 Forbidden**: Insufficient permissions (not Admin)
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server-side errors

## Testing the API

A test interface is available at `/api-test.html` when running the application. This provides a web-based interface to test all API endpoints.

### Using the Test Interface
1. Start the application
2. Navigate to `https://localhost:5001/api-test.html` (or your configured URL)
3. Use the forms to test different API endpoints
4. View responses in the response boxes

### Authentication for Admin Endpoints
To test admin-only endpoints (POST, PUT, DELETE), you need to:
1. Log in to the application as an Admin user
2. The authentication cookie will be automatically included in API requests

## Example Usage with JavaScript

```javascript
// Get all products
const response = await fetch('/api/ProductsApi?page=1&pageSize=10');
const data = await response.json();

// Create a new product (requires admin authentication)
const newProduct = {
    title: "New Account",
    description: "Account description",
    price: 1000000,
    categoryId: 1,
    isAvailable: true
};

const createResponse = await fetch('/api/ProductsApi', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
    },
    body: JSON.stringify(newProduct)
});

const result = await createResponse.json();
```

## Notes
- All prices are in Vietnamese Dong (VND)
- Dates are in ISO 8601 format
- The API uses Entity Framework Core with MySQL database
- Pagination is 1-based (first page is page 1, not 0)
- Search functionality is case-sensitive and uses SQL LIKE operator 