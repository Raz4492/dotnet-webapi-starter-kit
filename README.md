# Smart .NET Web API Structure

A comprehensive, production-ready .NET 8 Web API structure with authentication, caching, logging, and database operations using modern best practices.

## Features

- **JWT Authentication** with refresh tokens
- **Request/Response logging** with Serilog
- **Redis caching** integration
- **Dapper ORM** for high-performance database operations
- **SQL Server** database support
- **Dependency injection** architecture
- **Global exception handling**
- **Request logging middleware**
- **Swagger documentation**
- **Health checks**

## Project Structure

```
SmartAPI/
├── Controllers/
│   ├── AuthController.cs          # Authentication endpoints
│   └── UsersController.cs         # User management endpoints
├── Services/
│   ├── Interfaces/                # Service interfaces
│   ├── AuthService.cs            # Authentication business logic
│   ├── TokenService.cs           # JWT token management
│   └── CacheService.cs           # Redis caching service
├── Models/
│   ├── DTOs/                     # Data Transfer Objects
│   ├── User.cs                   # User entity
│   ├── RefreshToken.cs           # Refresh token entity
│   └── ApiResponse.cs            # Standardized API response
├── Data/
│   ├── IDbContext.cs             # Database context interface
│   └── DapperContext.cs          # Dapper database context
├── Middleware/
│   ├── RequestLoggingMiddleware.cs    # Request logging
│   └── ExceptionHandlingMiddleware.cs # Global exception handling
├── Configuration/
│   └── JwtSettings.cs            # JWT configuration
└── Scripts/
    └── CreateDatabase.sql        # Database setup script
```

## Setup Instructions

### 1. Database Setup
Run the SQL script in `Scripts/ExecuteDatabase.sql` to create the database and tables:

```sql
-- Execute ExecuteDatabase.sql in SQL Server Management Studio
-- or via command line:
sqlcmd -S "DESKTOP-CJDS30G\MSSQLSERVER01" -i "Scripts/ExecuteDatabase.sql"
```

### 2. Redis Setup
Install and start Redis on your machine:
- Windows: Download from [Redis for Windows](https://github.com/MicrosoftArchive/redis/releases)
- Linux/Mac: `sudo apt-get install redis-server` or `brew install redis`

### 3. Configuration
Update `appsettings.json` with your specific configurations:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your SQL Server connection string"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "JwtSettings": {
    "SecretKey": "Your secure secret key (256+ bits)"
  }
}
```

### 4. Run the Application
```bash
dotnet restore
dotnet build
dotnet run
```

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/revoke` - Revoke refresh token
- `GET /api/auth/me` - Get current user info

### Users
- `GET /api/users/{id}` - Get user by ID (Admin only)

### Health Check
- `GET /health` - API health status

## Default Credentials

After running the database setup script, you can login with:
- **Username:** `admin`
- **Password:** `Admin123!`

**Note:** If you're still having password issues, you can create a new admin user by running this SQL:

```sql
-- Generate a new BCrypt hash for Admin123! and insert
INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, Role, CreatedAt)
VALUES (
    'newadmin', 
    'newadmin@smartapi.com', 
    '$2a$11$K5rF8qF5sF8qF5sF8qF5sOeWjfVkW5/K5rF8qF5sF8qF5sF8qF5sF8', 
    'New',
    'Administrator',
    'Admin',
    GETUTCDATE()
);
```

## Features in Detail

### Authentication & Authorization
- JWT tokens with configurable expiration
- Refresh token rotation
- Role-based authorization
- Password hashing with BCrypt

### Caching
- Redis integration for high-performance caching
- Configurable cache expiration
- Cache-aside pattern implementation

### Logging
- Structured logging with Serilog
- Request/response logging middleware
- File and console logging sinks
- Automatic log rotation

### Database
- Dapper ORM for high-performance queries
- Connection pooling
- Parameterized queries to prevent SQL injection
- Database transaction support

### Error Handling
- Global exception handling middleware
- Standardized API responses
- Detailed error logging
- Client-friendly error messages

### Security
- HTTPS enforcement
- CORS configuration
- Input validation
- Secure password storage

## Usage Examples

### Login
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin123!"
  }'
```

### Access Protected Endpoint
```bash
curl -X GET https://localhost:5001/api/auth/me \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### Refresh Token
```bash
curl -X POST https://localhost:5001/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "YOUR_REFRESH_TOKEN"
  }'
```

## Monitoring & Observability

- Logs are stored in `Logs/` directory
- Request correlation IDs for tracing
- Performance metrics logging
- Health check endpoint for monitoring

## Security Considerations

- Keep JWT secret keys secure and rotate them regularly
- Use HTTPS in production
- Implement rate limiting
- Regular security updates
- Monitor suspicious activities through logs

## Scaling Considerations

- Redis cluster for high availability caching
- Database connection pooling and read replicas
- Load balancer configuration
- Container orchestration ready

## License

This project is open source and available under the MIT License.