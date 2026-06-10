# ResourceManagement API

A production-ready RESTful API for managing **Products** and **Items**, built with .NET 7 and ASP.NET Core Web API following Clean Architecture principles.

---

## 🏗️ Architecture

```
ResourceManagement.sln
├── src/
│   ├── ResourceManagement.Domain          ← Entities, Interfaces (no dependencies)
│   ├── ResourceManagement.Application     ← Services, DTOs, Validators, AutoMapper
│   ├── ResourceManagement.Infrastructure  ← EF Core, Repositories, UnitOfWork
│   └── ResourceManagement.API             ← Controllers, Middleware, Program.cs
└── tests/
    └── ResourceManagement.Tests           ← xUnit + Moq unit tests
```

## 🛠️ Technology Stack

| Technology | Purpose |
|---|---|
| .NET 7 / ASP.NET Core | Web API framework |
| Entity Framework Core 7 | ORM |
| SQL Server (LocalDB) | Database |
| JWT Bearer | Authentication |
| FluentValidation | Request validation |
| AutoMapper | Object mapping |
| Serilog | Structured logging |
| Swagger / Swashbuckle | API documentation |
| xUnit + Moq | Unit testing |
| Docker | Containerization |

---

## 🗄️ Database Schema

### Product
| Column | Type | Notes |
|---|---|---|
| Id | int | PK, auto-increment |
| ProductName | nvarchar(200) | Required |
| CreatedBy | nvarchar(100) | Required |
| CreatedOn | datetime2 | Required |
| ModifiedBy | nvarchar(100) | Nullable |
| ModifiedOn | datetime2 | Nullable |

### Item
| Column | Type | Notes |
|---|---|---|
| Id | int | PK, auto-increment |
| ProductId | int | FK → Product |
| Quantity | int | Required |

---

## 🚀 Getting Started

### Prerequisites
- .NET 7 SDK
- SQL Server LocalDB (comes with Visual Studio)
- Docker (optional)

### Run Locally

```bash
cd src/ResourceManagement.API
dotnet run
```

The API will:
1. Automatically apply EF Core migrations
2. Seed the `ResourceDb` database with sample data
3. Start at `https://localhost:{port}`
4. Open Swagger UI at `https://localhost:{port}` (root)

### Database Configuration

The connection string is in `src/ResourceManagement.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ResourceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

---

## 🔐 Authentication

The API uses **JWT Bearer authentication** and role-based authorization. Configure the JWT settings in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "ResourceManagement_SuperSecretKey_2024_MinLength32Chars!",
    "Issuer": "ResourceManagement.API",
    "Audience": "ResourceManagement.Client",
    "ExpiryMinutes": 60
  }
}
```

### Mock User Credentials

For testing and verification, the following hardcoded users are available:

| Username | Password | Role | Permissions |
| :--- | :--- | :--- | :--- |
| `admin` | `Admin@123` | `Admin` | Full access (GET, POST, PUT, DELETE) |
| `user` | `User@123` | `User` | Read-only access (GET only) |

### How to Authenticate
1. Call `POST /api/v1/auth/token` with the JSON payload:
   ```json
   {
     "username": "admin",
     "password": "Admin@123"
   }
   ```
2. Copy the token string returned under `data.token`.
3. In Swagger UI or your HTTP client, include the token in the `Authorization` header:
   ```
   Authorization: Bearer {your-copied-jwt-token}
   ```

---

## 📡 API Endpoints

Base URL: `/api/v1`

### Authentication

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| POST | `/api/v1/auth/token` | Generate JWT token | No |

### Products

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| GET | `/api/v1/products` | List all (paged, filtered, sorted) | Yes (Admin, User) |
| GET | `/api/v1/products/{id}` | Get by ID | Yes (Admin, User) |
| POST | `/api/v1/products` | Create product | Yes (Admin only) |
| PUT | `/api/v1/products/{id}` | Update product | Yes (Admin only) |
| DELETE | `/api/v1/products/{id}` | Delete product | Yes (Admin only) |

**Query Parameters (GET /products):**
- `filter` — filter by product name
- `sortBy` — field to sort by (`productName`, `createdOn`)
- `sortDescending` — true/false
- `page` — page number (default: 1)
- `pageSize` — items per page (default: 10)

### Items

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| GET | `/api/v1/items/by-product/{productId}` | Get items by product | Yes (Admin, User) |
| GET | `/api/v1/items/{id}` | Get item by ID | Yes (Admin, User) |
| POST | `/api/v1/items` | Create item | Yes (Admin only) |
| PUT | `/api/v1/items/{id}` | Update item | Yes (Admin only) |
| DELETE | `/api/v1/items/{id}` | Delete item | Yes (Admin only) |

### Standardized Response Format

```json
{
  "success": true,
  "message": "Operation message",
  "data": { ... },
  "errors": null
}
```

---

## 🧪 Running Tests

```bash
dotnet test tests/ResourceManagement.Tests/ResourceManagement.Tests.csproj --verbosity normal
```

Tests cover:
- ✅ ProductService (7 tests)
- ✅ ItemService (8 tests)
- ✅ ProductRepository (6 tests — in-memory EF)
- ✅ ProductsController (8 tests)
- ✅ ItemsController (9 tests)
- ✅ AuthController (3 tests)

---

## 🐳 Docker

### Build & Run

```bash
docker-compose up --build
```

This starts:
- API on `http://localhost:8080`
- SQL Server on port `1433`

### Individual Build

```bash
docker build -t resourcemanagement-api .
docker run -p 8080:80 resourcemanagement-api
```

---

## 📁 Logging

Logs are written to:
- **Console** (structured)
- **Rolling file**: `logs/api-{date}.log`

---

## 🔒 Security Features

- JWT Bearer token validation
- HTTPS redirection
- CORS policy
- Security headers (X-Content-Type-Options, X-Frame-Options, X-XSS-Protection)
- Global exception handling (no stack trace leakage)
