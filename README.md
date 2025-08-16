# Currency Service API

A robust, scalable, and maintainable currency conversion API built with C# and ASP.NET Core 8.0, providing high performance, security, and resilience. The service integrates with the Frankfurter API to deliver real-time and historical exchange rates with comprehensive error handling and monitoring.

## Features

- **Latest Exchange Rates**: Fetch the latest exchange rates for a specific base currency
- **Currency Conversion**: Convert amounts between different currencies with real-time rates
- **Historical Exchange Rates**: Retrieve historical exchange rates for a given period (Admin role required)
- **Resilience & Performance**:
  - Caching to minimize direct calls to the Frankfurter API
  - Retry policies with exponential backoff
  - Circuit breaker pattern for graceful handling of API outages
- **Security & Access Control**:
  - JWT authentication with role-based access control (RBAC)
  - API throttling to prevent abuse
  - Restricted currency handling (TRY, PLN, THB, MXN are blocked)
- **Logging & Monitoring**:
  - Structured logging with Serilog
  - Seq integration for centralized log management
  - Request/response correlation
  - Detailed request logging (client IP, client ID, method, endpoint, response code, response time)
- **API Versioning**: Full support for API versioning with backward compatibility

## Architecture

The project follows Clean Architecture principles with the following layers:

- **API Layer** (`CurrencyService.Api`): Controllers, middleware, and API models
- **Application Layer** (`CurrencyService.Application`): Business logic, CQRS with MediatR, and application services
- **Domain Layer** (`CurrencyService.Domain`): Domain models, interfaces, and business rules
- **Infrastructure Layer** (`CurrencyService.Infrasturucture`): External service integrations, caching, security, and data access

### Why This Architecture for a Currency Service API?

This architecture was specifically chosen for a currency service API for several key benefits:

1. **Separation of Concerns**: Each layer has a distinct responsibility, making the codebase more maintainable and easier to understand. For a currency service that needs to handle various exchange rate providers, conversion logic, and API endpoints, this clear separation helps manage complexity.

2. **Dependency Rule**: Dependencies flow inward, with the Domain layer having no dependencies on outer layers. This ensures that business logic remains independent of implementation details. This is particularly valuable for a currency service that might need to switch between different exchange rate providers without affecting core conversion logic.

3. **Testability**: The use of interfaces and dependency injection makes the code highly testable, allowing for effective unit testing without external dependencies. For financial applications like currency services, high test coverage is critical to ensure accurate calculations and reliable service.

4. **Flexibility**: The architecture allows for easy replacement of external components (like the currency provider) without affecting the core business logic. This is essential for a currency service that needs to maintain service continuity even if an external provider becomes unavailable.

5. **Scalability**: As the application grows, new features can be added without significant refactoring of existing code. The currency service might need to add features like historical rate analysis, currency alerts, or support for cryptocurrencies in the future.

6. **Resilience**: By isolating external dependencies in the Infrastructure layer, the application can implement resilience patterns (retry, circuit breaker, fallback) to handle external service failures gracefully. This is crucial for a currency service that depends on third-party APIs for rate data.

7. **Security**: The layered approach allows for implementing security concerns at appropriate levels - authentication and authorization at the API layer, while keeping business rules protected in the Domain layer. For financial data, this separation helps maintain proper security boundaries.

### Key Design Patterns

- **CQRS Pattern**: Command Query Responsibility Segregation using MediatR
- **Factory Pattern**: Used for creating appropriate currency providers
- **Dependency Injection**: Used throughout the application for loose coupling
- **Options Pattern**: Used for configuration management (e.g., restricted currencies, JWT settings)
- **Pipeline Behavior**: Cross-cutting concerns like validation using MediatR pipeline behaviors

## Technology Stack

- **.NET 8.0**: Latest LTS version with performance improvements
- **ASP.NET Core**: Modern web framework with built-in dependency injection
- **MediatR**: Implementation of CQRS pattern and mediator pattern
- **Mapster**: High-performance object mapping library
- **Polly**: Resilience and transient-fault-handling capabilities
- **Serilog**: Structured logging with multiple sinks
- **Seq**: Centralized log management and analysis
- **JWT Bearer**: Secure authentication mechanism
- **Swagger/OpenAPI**: API documentation and testing interface
- **Docker**: Containerization for consistent deployment

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or any preferred IDE
- Docker Desktop (for containerized deployment)

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/yourusername/CurrencyService.git
   ```

2. Navigate to the project directory:

   ```bash
   cd CurrencyService
   ```

3. Restore dependencies:

   ```bash
   dotnet restore
   ```

4. Build the solution:

   ```bash
   dotnet build
   ```


### Running with Docker Compose

For a complete development environment with logging:

```bash
docker-compose up -d
```
For stopping server

```bash
docker-compose down
```

The API will be available at `https://localhost:5001` and `http://localhost:5000`.

This will start:

- Currency Service API on ports 5000 (HTTP) and 5001 (HTTPS)
- Seq logging ui server on port 5342

![Logs](image.png)

## API Endpoints

### Authentication

#### Login

```http
POST /api/v1/auth/login
```

**Request Body:**

```json
{
  "username": "user1",
  "password": "password123"
}
```

**Response (200 OK):**

```json
{
  "username": "user1",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "roles": ["User"]
}
```

**Available Users:**

- Regular User:
  - Username: `user1`
  - Password: `password123`
  - Roles: `User`
- Admin User:
  - Username: `admin`
  - Password: `admin123`
  - Roles: `Admin`

**Note:** You can use the provided users for testing or development purposes. For production, you should implement proper user management and authentication.

### Currency Operations

All currency endpoints require authentication. Include the JWT token in the Authorization header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Get Latest Exchange Rates

Retrieves the latest exchange rates for a specified base currency.

```http
GET /api/v1/currency/latest-rates/{baseCurrency}
```

**Parameters:**

- `baseCurrency` (path parameter): The base currency code (e.g., USD, EUR, GBP)

**Example Request:**

```http
GET /api/v1/currency/latest-rates/USD
```

**Response (200 OK):**

```json
{
  "amount": 1,
  "base": "USD",
  "date": "2025-01-14T00:00:00",
  "rates": {
    "EUR": 0.91,
    "GBP": 0.78,
    "JPY": 153.42,
    "CAD": 1.36,
    "AUD": 1.51
  }
}
```

**Response (400 Bad Request):**

```json
{
  "statusCode": 400,
  "message": "The currency code 'TRY' is not supported.",
  "errors": null
}
```

#### Convert Currency

Converts an amount from one currency to another.

```http
POST /api/v1/currency/convert
```

**Request Body:**

```json
{
  "amount": 100,
  "fromCurrency": "USD",
  "toCurrency": "EUR"
}
```

**Response (200 OK):**

```json
{
  "amount": 100,
  "fromCurrency": "USD",
  "toCurrency": "EUR",
  "convertedAmount": 91.02,
  "date": "2025-01-14T00:00:00",
  "rate": 0.9102
}
```

#### Get Historical Rates

Retrieves historical exchange rates for a specified period. This endpoint requires Admin role.

```http
GET /api/v1/currency/historical-rates?baseCurrency={baseCurrency}&startDate={startDate}&endDate={endDate}&page={page}&pageSize={pageSize}
```

**Parameters:**

- `baseCurrency` (required): The base currency code
- `startDate` (required): Start date in ISO format (YYYY-MM-DD)
- `endDate` (required): End date in ISO format (YYYY-MM-DD)
- `page` (optional): Page number for pagination (default: 1)
- `pageSize` (optional): Number of items per page (default: 10)

**Example Request:**

```http
GET /api/v1/currency/historical-rates?baseCurrency=EUR&startDate=2025-01-01&endDate=2025-01-31&page=1&pageSize=5
```

## Configuration

### Appsettings Configuration

The application uses configuration files for various settings:

**Development Configuration (`appsettings.Development.json`):**

```json
{
  "FrankfurtApi": {
    "BaseUrl": "https://api.frankfurter.app"
  },
  "UsersData": {
    "Users": [
      {
        "UserName": "user1",
        "Password": "password123",
        "Roles": ["User"]
      },
      {
        "UserName": "admin",
        "Password": "admin123",
        "Roles": ["Admin"]
      }
    ]
  },
  "Jwt": {
    "Key": "supersecretkey1234567890supersecretkey12345678901234567890",
    "Issuer": "MyApi",
    "Audience": "MyApiUsers"
  }
}
```

### Restricted Currencies

The following currencies are restricted and cannot be used:

- TRY (Turkish Lira)
- PLN (Polish Złoty)
- THB (Thai Baht)
- MXN (Mexican Peso)

## Testing

The project includes a comprehensive testing strategy with multiple test types:

### Unit Tests

Unit tests focus on testing individual components in isolation, using mocks for dependencies. The project includes:

- **Service Tests**: Verify the business logic in the Application layer
- **Provider Tests**: Ensure correct interaction with external APIs
- **Error Handling Tests**: Validate proper exception handling
- **Edge Case Tests**: Test boundary conditions and input validation

### Integration Tests

Integration tests verify that different components work correctly together:

- **API Provider Tests**: Test the actual integration with the Frankfurter API
- **End-to-End Flows**: Verify complete business processes from request to response

![Test Coverage](coverage.png)

## Error Handling

The API uses a global exception handling middleware that returns consistent error responses:

```json
{
  "statusCode": 400,
  "message": "Error message describing the issue",
  "errors": null
}
```

Multiple validation errors

```json
{
  "message": "Multiple errors occurred.",
  "statusCode": 400,
  "errors": [
    "Base currency must be exactly 3 characters long.",
    "From currency must consist of 3 uppercase letters.",
    "Base currency must be exactly 3 characters long.",
    "To currency must consist of 3 uppercase letters.",
    "Amount must be greater than zero."
  ]
}
```

Common HTTP status codes:

- `200 OK`: Request succeeded
- `400 Bad Request`: Invalid input or validation error
- `401 Unauthorized`: Missing or invalid authentication
- `403 Forbidden`: Authenticated but not authorized for the resource
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Unexpected server error

## API Client Examples

### cURL

**Login:**

```bash
curl -X POST "https://localhost:5001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"user1","password":"password123"}'
```

**Get Latest Rates:**

```bash
curl -X GET "https://localhost:5001/api/v1/currency/latest-rates/USD" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Convert Currency:**

```bash
curl -X POST "https://localhost:5001/api/v1/currency/convert" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{"amount":100,"fromCurrency":"USD","toCurrency":"EUR"}'
```

**Get Historical Rates:**

```bash
curl -X GET "https://localhost:5001/api/v1/currency/historical-rates?baseCurrency=EUR&startDate=2025-01-01&endDate=2025-01-31&page=1&pageSize=5" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```


## Assumptions

1. The Frankfurter API is the primary data source for exchange rates
2. Restricted currencies (TRY, PLN, THB, MXN) are managed through configuration rather than hardcoded
3. JWT authentication is sufficient for the API's security requirements
4. In-memory caching is adequate for the current scale of operations
5. The application requires high test coverage to ensure reliability
6. Seq logging provides centralized log management for development and debugging

## Future Enhancements

1. **Multiple Exchange Rate Providers**:

   - Add support for additional providers (e.g., Open Exchange Rates, Fixer.io)
   - Implement provider fallback strategies

2. **Advanced Caching**:

   - Implement distributed caching with Redis
   - Add cache invalidation strategies
   - Implement cache warming for frequently accessed rates

3. **Enhanced Security**:

   - Implement OAuth 2.0 / OpenID Connect
   - Add API key management
   - Implement IP-based restrictions
   - Add rate limiting per user/role

4. **Monitoring & Observability**:

   - Integrate with Application Performance Monitoring (APM) tools
   - Add health checks and readiness probes
   - Implement metrics collection for business KPIs
   - Add distributed tracing with OpenTelemetry

5. **Deployment & Scalability**:

   - Set up Kubernetes deployment
   - Implement auto-scaling based on load
   - Add blue-green deployment strategies
   - Implement feature flags for gradual rollouts

6. **Data Persistence**:
   - Add database support for user management
   - Implement audit logging for currency conversions
   - Add support for custom user preferences

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

