# 🍔 FoodDelivery — Microservices Solution

A cloud-ready, .NET 10 food delivery platform built with a **Clean Architecture** microservices pattern.  
Services communicate asynchronously via **RabbitMQ** and are routed through an **Ocelot API Gateway**.

---

## 🏗️ Solution Structure

```
FoodDelivery.sln
│
├── Gateway/
│   └── FoodDelivery.Gateway          ← Ocelot reverse-proxy / API Gateway
│
├── Shared/
│   └── FoodDelivery.Shared           ← Shared contracts, events, common utilities
│
├── Services/
│   ├── AuthService/                  ← JWT authentication & user management
│   ├── CatalogService/               ← Restaurants, menus & food items
│   ├── OrderService/                 ← Order lifecycle management
│   ├── PaymentService/               ← Payment processing & transactions
│   └── AdminService/                 ← Back-office administration
│
├── docs/                             ← Architecture diagrams & documentation
├── docker-compose.yml                ← Full stack local environment
└── README.md
```

Each service follows the same **layered Clean Architecture**:

| Layer | Project | Responsibilities |
|---|---|---|
| Domain | `*.Domain` | Entities, Interfaces, Enums |
| Application | `*.Application` | DTOs, Services, Interfaces, Exceptions |
| Infrastructure | `*.Infrastructure` | Persistence (EF Core), Repositories |
| Presentation | `*.API` | Controllers, Middleware, Program.cs |
| Tests | `*.Tests` | NUnit unit & integration tests |

---

## 🛠️ Tech Stack

| Concern | Technology |
|---|---|
| Framework | .NET 10 / ASP.NET Core |
| API Gateway | Ocelot |
| Message Broker | RabbitMQ 3.13 |
| ORM | Entity Framework Core |
| Database | SQL Server 2022 |
| Testing | NUnit |
| Containerisation | Docker / Docker Compose |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Run locally with Docker Compose

```bash
docker compose up --build
```

| Service | URL |
|---|---|
| API Gateway | http://localhost:5000 |
| Auth API | http://localhost:5001 |
| Catalog API | http://localhost:5002 |
| Order API | http://localhost:5003 |
| Admin API | http://localhost:5004 |
| Payment API | http://localhost:5005 |
| RabbitMQ UI | http://localhost:15672 |

> Default RabbitMQ credentials: `guest / guest`  
> Default SQL Server SA password: `YourStrong!Passw0rd`

### Run without Docker

```bash
# Restore all packages
dotnet restore FoodDelivery.slnx

# Build entire solution
dotnet build FoodDelivery.slnx

# Run a specific service (example)
dotnet run --project Services/AuthService/AuthService.API/AuthService.API.csproj
```

### Run Tests

```bash
dotnet test FoodDelivery.slnx
```

---

## 📡 Service Responsibilities

### 🔐 AuthService
Handles user registration, login, JWT token issuance, and refresh token management.

### 🍽️ CatalogService
Manages restaurants, categories, menu items, and pricing. Read-heavy service with caching support.

### 📦 OrderService
Orchestrates the full order lifecycle — creation, confirmation, preparation, and delivery tracking.

### 💳 PaymentService
Processes payments, manages transaction records, and publishes payment events to RabbitMQ.

### 🛡️ AdminService
Provides back-office functionality for managing users, restaurants, and platform configuration.

---

## 📬 Messaging (RabbitMQ Events)

Events are defined in `FoodDelivery.Shared` and consumed across services:

| Event | Publisher | Consumer(s) |
|---|---|---|
| `OrderPlacedEvent` | OrderService | PaymentService, CatalogService |
| `PaymentCompletedEvent` | PaymentService | OrderService |
| `PaymentFailedEvent` | PaymentService | OrderService |
| `UserRegisteredEvent` | AuthService | AdminService |

---

## � Implementation Progress

### CatalogService Microservice

| Phase | Component | Status | Details |
|-------|-----------|--------|---------|
| **Phase 1** | Domain Layer | ✅ Complete | 10 files (BaseEntity, Enums, Entities, Interfaces) |
| **Phase 2** | Application Layer | ✅ Complete | 31 files (DTOs, Services, Exceptions, Profiles, AutoMapper) |
| **Phase 3** | Infrastructure Layer | ✅ Complete | 7 files (DbContext, Repositories, DbContextFactory, Seeder) + Migrations applied |
| **Phase 4** | API Layer (Controllers) | 🔄 In Progress | 4 controllers, 20 endpoints, JWT auth, error middleware |
| **Phase 5** | Integration & Messaging | ⏳ Planned | RabbitMQ events, inter-service communication |

### Phase 4: API Layer

**Implementation Details:**
- **4 Controllers:** RestaurantsController, MenuItemsController, CategoriesController, SearchController
- **20 RESTful Endpoints:** CRUD operations + advanced search
- **Middleware:** GlobalExceptionHandlingMiddleware for consistent error handling
- **Authentication:** JWT Bearer tokens on all endpoints
- **Authorization:** Admin-only for write operations
- **CORS:** Enabled for React frontend (http://localhost:3000)
- **API Documentation:** Swagger/OpenAPI with interactive UI

---

## �📁 Docs

See the [`docs/`](./docs/) folder for architecture diagrams and API documentation.

---

## 📄 License

This project is licensed under the MIT License.
