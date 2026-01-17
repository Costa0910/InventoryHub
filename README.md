# InventoryHub+ 🚀

A polished, production-style inventory management system built with **Blazor WebAssembly** + **.NET Minimal APIs**. Demonstrates clean architecture, API integration, state management, and performance optimization.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Features](#features)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Setup & Running](#setup--running)
- [API Endpoints](#api-endpoints)
- [Performance Optimizations](#performance-optimizations)
- [Future Enhancements](#future-enhancements)
- [⚠️ Important Configuration](#important-configuration)

---

## Overview

**InventoryHub+** is a full-stack inventory management application that allows users to:

- Manage products with CRUD operations
- Organize products into categories
- Search and filter products with debounced input
- Track stock levels
- Persist filters/pagination in URL for shareable views
- See real-time loading feedback while fetching data

Built with modern web standards and best practices, this project demonstrates how to build a scalable, maintainable SPA that integrates tightly with a RESTful API backend.

---

## Tech Stack

### Backend
- **Framework**: .NET 10 (minimal APIs)
- **Database**: SQLite (Entity Framework Core)
- **ORM**: Entity Framework Core
- **Architecture**: Clean Architecture with Repository Pattern
- **Caching**: In-memory caching for full lists

### Frontend
- **Framework**: Blazor WebAssembly
- **Language**: C#
- **Styling**: Bootstrap 5
- **State Management**: Custom service-based state (ProductState, CategoryState)
- **HTTP Client**: HttpClient with error handling middleware
- **Performance**: Debounced search, client-side paging cache, URL persistence

### DevOps & Tools
- **Version Control**: Git
- **Build System**: dotnet CLI
- **Testing**: xUnit, MSTest

---

## Features

### 1. Product Management (CRUD)
- ✅ **View** all products with pagination
- ✅ **Search** by name or description (debounced)
- ✅ **Filter** by category (real-time backend filtering)
- ✅ **Add** new products via modal form
- ✅ **Edit** existing products with validation
- ✅ **Delete** products with confirmation
- ✅ **View details** in a detailed modal

### 2. Category Management (CRUD)
- ✅ **View** all categories with pagination
- ✅ **Search** categories by name (debounced)
- ✅ **Add** new categories
- ✅ **Edit** categories
- ✅ **Delete** categories with confirmation
- ✅ Products automatically link to categories via dropdown

### 3. Search & Filtering
- ✅ **Debounced search**: Reduces API calls by waiting 350ms after typing stops
- ✅ **Category filter**: Instant server-side filtering when category is selected
- ✅ **Pagination**: Navigate pages with Previous/Next and page number buttons
- ✅ **Empty states**: Friendly messages when no results are found

### 4. Performance Enhancements
- ✅ **Client-side paging cache**: Repeated identical page requests served from memory (30s TTL)
- ✅ **Cache invalidation**: Automatic cache clearing on create/delete operations
- ✅ **Debounced search**: 350ms delay before API call to reduce server load
- ✅ **Lazy loading**: Categories loaded on-demand and cached in CategoryState
- ✅ **URL persistence**: Search, category, and page stored in query string for shareable views
- ✅ **Server-side filtering**: Backend applies search & category filters before pagination

### 5. UX Improvements
- ✅ **Loading spinner**: Visual feedback during API calls (LoadingSpinner component)
- ✅ **Empty state messages**: Helpful text when no products/categories exist
- ✅ **Disabled controls during loading**: Select dropdowns disabled while fetching
- ✅ **Toast notifications**: Success/error messages for all operations
- ✅ **Responsive design**: Mobile-friendly Bootstrap layout

---

## Architecture

### High-Level Flow

```
┌─────────────────────────────────────────────────────────────┐
│                     Client (Blazor WASM)                    │
│  ┌─────────────────────────────────────────────────────────┐│
│  │ Pages: Products.razor, Categories.razor                 ││
│  │ ├─ UI Components (modals, forms, spinners)              ││
│  │ ├─ State Management (ProductState, CategoryState)       ││
│  │ └─ Event Handlers & Debounce Logic                      ││
│  └─────────────────────────────────────────────────────────┘│
│  ┌─────────────────────────────────────────────────────────┐│
│  │ Services & HttpClient                                   ││
│  │ ├─ ProductApiService (with paging cache)               ││
│  │ ├─ CategoryApiService (with paging cache)              ││
│  │ ├─ NotificationService (toast messages)                ││
│  │ └─ ApiServiceBase (generic HTTP logic)                 ││
│  └─────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
           │ HTTPS (REST API calls)
           │ Query String (pagination, search, filters)
           │
┌──────────────────────────────────────────────────────────────┐
│                Server (.NET Minimal APIs)                    │
│  ┌──────────────────────────────────────────────────────────┐│
│  │ Endpoints: /api/products, /api/categories               ││
│  │ ├─ GET /?pageNumber=X&pageSize=Y&search=Q&categoryId=Z ││
│  │ ├─ GET /{id}                                            ││
│  │ ├─ POST / (create)                                      ││
│  │ ├─ PUT /{id} (update)                                   ││
│  │ └─ DELETE /{id}                                         ││
│  └──────────────────────────────────────────────────────────┘│
│  ┌──────────────────────────────────────────────────────────┐│
│  │ Application Layer (Services)                            ││
│  │ ├─ ProductService (CRUD, caching, validation)           ││
│  │ ├─ CategoryService (CRUD, caching, validation)          ││
│  │ └─ GlobalExceptionMiddleware (error handling)           ││
│  └──────────────────────────────────────────────────────────┘│
│  ┌──────────────────────────────────────────────────────────┐│
│  │ Data Layer (Repositories & EF Core)                     ││
│  │ ├─ ProductRepository (GetPagedAsync, filtering)         ││
│  │ ├─ CategoryRepository (GetPagedAsync, filtering)        ││
│  │ ├─ GenericRepository (base CRUD)                        ││
│  │ └─ InventoryDbContext (EF migrations)                   ││
│  └──────────────────────────────────────────────────────────┘│
│  ┌──────────────────────────────────────────────────────────┐│
│  │ Database (SQLite)                                       ││
│  │ ├─ Categories table                                     ││
│  │ └─ Products table (FK → Categories)                     ││
│  └──────────────────────────────────────────────────────────┘│
└──────────────────────────────────────────────────────────────┘
```

### Design Patterns Used

1. **Repository Pattern**: Data access abstraction via IProductRepository, ICategoryRepository
2. **Service Layer**: Business logic and validation in ProductService, CategoryService
3. **Dependency Injection**: All dependencies wired in Program.cs
4. **State Management**: Reactive state via ProductState and CategoryState event notifications
5. **Middleware**: GlobalExceptionMiddleware for consistent error handling
6. **Generic Base Classes**: ApiServiceBase<T> for code reuse in API clients

---

## Project Structure

```
InventoryHubPlus/
├── ClientApp/                          # Blazor WebAssembly client
│   ├── Pages/
│   │   ├── Products.razor             # Product CRUD + pagination
│   │   ├── Categories.razor           # Category CRUD + pagination
│   │   └── ...
│   ├── Components/
│   │   ├── LoadingSpinner.razor       # Reusable loading indicator
│   │   ├── Shared/                    # Layout & nav
│   │   ├── Products/                  # Product modals & forms
│   │   └── Categories/                # Category modals & forms
│   ├── Services/
│   │   ├── ProductApiService.cs       # Product API calls + paging cache
│   │   ├── CategoryApiService.cs      # Category API calls + paging cache
│   │   ├── ApiServiceBase.cs          # Generic HTTP logic
│   │   ├── ProductState.cs            # Product state management
│   │   ├── CategoryState.cs           # Category state management
│   │   ├── NotificationService.cs     # Toast notifications
│   │   └── ...error handlers, etc.
│   ├── Program.cs                     # WASM app setup
│   └── ...
│
├── ServerApp/                          # .NET Minimal API backend
│   ├── Endpoints/
│   │   ├── ProductEndpoints.cs        # GET /api/products (with paging)
│   │   └── CategoryEndpoints.cs       # GET /api/categories (with paging)
│   ├── Application/
│   │   ├── Services/
│   │   │   ├── ProductService.cs      # Product business logic
│   │   │   └── CategoryService.cs     # Category business logic
│   │   └── Interfaces/
│   │       ├── IProductService.cs
│   │       ├── IProductRepository.cs
│   │       └── ...
│   ├── Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── InventoryDbContext.cs  # EF DbContext
│   │   │   └── DbSeeder.cs            # 30 categories + 50 products
│   │   ├── Repositories/
│   │   │   ├── ProductRepository.cs   # GetPagedAsync, filtering
│   │   │   ├── CategoryRepository.cs  # GetPagedAsync, filtering
│   │   │   └── GenericRepository.cs   # Base CRUD
│   │   └── Migrations/                # EF migrations
│   ├── Domain/
│   │   ├── Product.cs                 # Product entity
│   │   └── Category.cs                # Category entity
│   ├── Middleware/
│   │   └── GlobalExceptionMiddleware.cs
│   ├── Program.cs                     # API app setup + middleware
│   └── ...
│
├── Shared/                             # Shared DTOs & models
│   ├── DTOs/
│   │   ├── ProductDto.cs
│   │   ├── CategoryDto.cs
│   │   ├── PaginatedResponse.cs       # Generic paging response
│   │   └── ...
│   └── Shared.csproj
│
├── ServerApp.Tests/                    # Unit & integration tests
│   ├── ProductServiceTests.cs
│   ├── CategoryServiceTests.cs
│   ├── ApiIntegrationTests.cs
│   └── ...
│
├── README.md                           # This file
├── REFLECTION.md                       # Copilot usage & lessons learned
└── InventoryHubPlus.sln               # Solution file
```

---

## Setup & Running

### Prerequisites

- **.NET 10 SDK** ([download](https://dotnet.microsoft.com/download))
- **Git** (optional, for version control)

### 1. Clone or Extract Repository

```bash
cd InventoryHub
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build Solution

```bash
dotnet build InventoryHubPlus.sln
```

### 4. Run Server App

In a terminal, run:

```bash
cd ServerApp
dotnet run
```

The server will start on `https://localhost:7058` by default. You should see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7058
```

The database will auto-seed with 30 categories and 50 products on first run.

### 5. Run Client App (in a separate terminal)

```bash
cd ClientApp
dotnet watch run
```

The client will compile and open in your default browser at `https://localhost:5173` (or similar).

### 6. Access the Application

Open your browser and navigate to:
- **Home**: `https://localhost:5173` (or the address shown in terminal)
- **Products**: `https://localhost:5173/products`
- **Categories**: `https://localhost:5173/categories`

---

## API Endpoints

All endpoints are **RESTful** and return JSON. Base URL: `https://localhost:7058/api`

### Products

#### Get Products (with Pagination & Filtering)
```http
GET /api/products
Query Parameters:
  - pageNumber (int, optional): Page number (default: no paging = return all)
  - pageSize (int, optional): Items per page (default: no paging = return all)
  - search (string, optional): Search by name or description
  - categoryId (int, optional): Filter by category ID

Response (Paged):
{
  "items": [
    {
      "id": 1,
      "name": "Portable Speaker 1",
      "description": "Sample product 1: Portable Speaker for everyday use.",
      "price": 7.35,
      "stock": 13,
      "categoryId": 1,
      "categoryName": "Electronics"
    },
    ...
  ],
  "totalCount": 50
}

Response (Unpaged, no query params):
[
  { "id": 1, "name": "...", ... },
  { "id": 2, "name": "...", ... },
  ...
]
```

#### Get Product by ID
```http
GET /api/products/{id}

Response:
{
  "id": 1,
  "name": "Portable Speaker 1",
  "description": "Sample product 1: Portable Speaker for everyday use.",
  "price": 7.35,
  "stock": 13,
  "categoryId": 1,
  "categoryName": "Electronics"
}
```

#### Create Product
```http
POST /api/products
Content-Type: application/json

Request Body:
{
  "name": "New Product",
  "description": "A description",
  "price": 29.99,
  "stock": 100,
  "categoryId": 1
}

Response:
{
  "id": 51,
  "name": "New Product",
  "description": "A description",
  "price": 29.99,
  "stock": 100,
  "categoryId": 1,
  "categoryName": "Electronics"
}
```

#### Update Product
```http
PUT /api/products/{id}
Content-Type: application/json

Request Body:
{
  "id": 1,
  "name": "Updated Name",
  "description": "Updated description",
  "price": 39.99,
  "stock": 50,
  "categoryId": 2
}

Response:
{
  "id": 1,
  "name": "Updated Name",
  "description": "Updated description",
  "price": 39.99,
  "stock": 50,
  "categoryId": 2,
  "categoryName": "Office"
}
```

#### Delete Product
```http
DELETE /api/products/{id}

Response: 200 OK (no body)
```

---

### Categories

#### Get Categories (with Pagination & Filtering)
```http
GET /api/categories
Query Parameters:
  - pageNumber (int, optional): Page number (default: no paging = return all)
  - pageSize (int, optional): Items per page (default: no paging = return all)
  - search (string, optional): Search by name

Response (Paged):
{
  "items": [
    { "id": 1, "name": "Electronics" },
    { "id": 2, "name": "Office" },
    ...
  ],
  "totalCount": 30
}

Response (Unpaged):
[
  { "id": 1, "name": "Electronics" },
  { "id": 2, "name": "Office" },
  ...
]
```

#### Get Category by ID
```http
GET /api/categories/{id}

Response:
{
  "id": 1,
  "name": "Electronics"
}
```

#### Create Category
```http
POST /api/categories
Content-Type: application/json

Request Body:
{
  "name": "New Category"
}

Response:
{
  "id": 31,
  "name": "New Category"
}
```

#### Update Category
```http
PUT /api/categories/{id}
Content-Type: application/json

Request Body:
{
  "id": 1,
  "name": "Updated Category Name"
}

Response:
{
  "id": 1,
  "name": "Updated Category Name"
}
```

#### Delete Category
```http
DELETE /api/categories/{id}

Response: 200 OK (no body)
```

---

## Performance Optimizations

### 1. **Debounced Search**
- Waits **350ms** after user stops typing before issuing API call
- Reduces server load during rapid typing
- Implemented in `Products.razor` and `Categories.razor` using `CancellationTokenSource`

### 2. **Client-Side Paging Cache**
- Caches paged API responses in memory with **30-second TTL**
- Repeated identical requests (same page, search, filter) serve from cache
- Automatic invalidation on create/delete operations
- Implemented in `ProductApiService` and `CategoryApiService` using `ConcurrentDictionary`

### 3. **Server-Side Filtering & Pagination**
- Backend applies `search` and `categoryId` filters **before** pagination
- Uses EF Core `Skip()` and `Take()` for efficient database queries
- Returns `PaginatedResponse<T>` with item count for accurate pagination UI

### 4. **Lazy Category Loading**
- Categories loaded on-demand and cached in `CategoryState`
- Subsequent page visits reuse cache; no redundant API calls
- Products dropdown populates from cache, not a separate API

### 5. **URL Persistence**
- Filters (search, category, page) stored in URL query string
- Users can reload/back-forward and restore their view
- Shareable links preserve exact filter state

### 6. **In-Memory Caching (Server)**
- Full product/category lists cached in memory (60-second TTL)
- Cache automatically invalidated on create/update/delete
- Reduces database hits for non-paged requests

### 7. **Async/Await Throughout**
- All API calls, database queries, and I/O are fully async
- No blocking operations; UI remains responsive
- Proper cancellation support for debounced/deferred operations

## Testing

### Run Unit & Integration Tests

```bash
dotnet test ServerApp.Tests/ServerApp.Tests.csproj
```

Tests cover:
- ✅ ProductService CRUD & filtering
- ✅ CategoryService CRUD & duplicate prevention
- ✅ Repository pagination & search
- ✅ API endpoint responses
- ✅ Error handling & validation

---

## ⚠️ Important Configuration

### Base URLs & CORS Settings

By default, the application assumes:
- **Server API**: `https://localhost:7058/`
- **Client App**: `http://localhost:5054` (or `https://localhost:5173` for Blazor dev server)

If you're running the server or client on **different URLs** (e.g., different ports, remote machine, Docker), you **must** update the configuration. Failure to do so will result in CORS errors and failed API calls.

#### Client-Side Configuration

The client reads the API base URL from a JSON configuration file. Update it if your **server is on a different URL**:

**File**: `ClientApp/wwwroot/appsettings.json`

```json
{
  "ApiBaseUrl": "https://localhost:7058/"
}
```

Change the URL to match your server:
```json
{
  "ApiBaseUrl": "https://api.example.com/"
}

// or for a different port
{
  "ApiBaseUrl": "https://localhost:5000/"
}

// or for IP-based access
{
  "ApiBaseUrl": "http://192.168.1.100:7058/"
}
```

**How it's loaded in code** (`ClientApp/Program.cs`):
```csharp
// Reads from appsettings.json; falls back to https://localhost:7058/ if not found
var apiBase = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7058/";

// Registers the HttpClient with the configured base address
builder.Services.AddHttpClient("Api", client => client.BaseAddress = new Uri(apiBase))
    .AddHttpMessageHandler<ApiErrorHandler>();
```

#### Server-Side CORS Configuration

The server controls which client origins are allowed via CORS policy. Update it if your **client is on a different URL**:

**File**: `ServerApp/appsettings.json`

```json
{
  "CorsSettings": {
    "AllowedOrigins": [
      "http://localhost:5054"
    ]
  }
}
```

Add your client URL(s) to the `AllowedOrigins` array:
```json
{
  "CorsSettings": {
    "AllowedOrigins": [
      "http://localhost:5054",
      "https://app.example.com"
    ]
  }
}

// or for a different port
{
  "CorsSettings": {
    "AllowedOrigins": [
      "https://localhost:5173"
    ]
  }
}

// or for IP-based access
{
  "CorsSettings": {
    "AllowedOrigins": [
      "http://192.168.1.50:5173"
    ]
  }
}
```

**How it's used in code** (`ServerApp/Program.cs`):
```csharp
// Read CORS origins from appsettings.json
var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? [];

// Apply CORS policy with those origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ...later...

// Enable the CORS policy
app.UseCors("AllowFrontend");
```

### Database Configuration

By default, the server uses SQLite with a local database file:

**File**: `ServerApp/appsettings.json`

```json
{
  "ConnectionStrings": {
    "Inventory": "Data Source=inventory.db"
  }
}
```
