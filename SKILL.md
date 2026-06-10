# SKILL.md

# RESTful Product API - Development Guidelines & Technical Standards

## Project Objective

Build a production-ready RESTful API using .NET 8 and ASP.NET Core Web API that manages Products and related Items.

The solution must demonstrate:
- Clean Architecture
- SOLID Principles
- Repository Pattern
- Unit of Work Pattern
- JWT Authentication
- API Versioning
- FluentValidation
- Swagger Documentation
- Docker Support
- Unit Testing
- Structured Logging
- Scalability
- Maintainability
- Security Best Practices

## Business Requirements

The system must provide CRUD operations for Products and Items.

## Database Schema

### Product
- Id
- ProductName
- CreatedBy
- CreatedOn
- ModifiedBy
- ModifiedOn

### Item
- Id
- ProductId
- Quantity

## Architecture
- API Layer
- Application Layer
- Domain Layer
- Infrastructure Layer

## Technology Stack
- .NET 8
- ASP.NET Core Web API
- SQL Server
- Entity Framework Core
- JWT Authentication
- FluentValidation
- AutoMapper
- Swagger
- Serilog
- Docker
- xUnit
- Moq

## API Requirements
- RESTful design
- CRUD endpoints
- Pagination
- Filtering
- Sorting
- API Versioning
- Standardized responses
- Global exception handling

## Security
- JWT Access Token
- Refresh Token
- Role-based authorization
- HTTPS
- CORS
- Security headers

## Testing
- Unit Tests
- Integration Tests
- 80%+ coverage target

## Deployment
- Dockerfile
- Docker Compose
- CI/CD ready

## Deliverables
- Source Code
- Tests
- Swagger
- README
- Docker Setup
- Authentication
- Logging
- Validation
