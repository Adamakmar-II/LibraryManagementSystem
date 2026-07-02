# Library Management System API

A Library Management System Web API built with .NET 8, Entity Framework Core,
JWT Bearer Authentication, and Serilog logging.
Implements basic CRUD (Create, Read, Update, Delete) operations
for managing books in a library system.

---

## Tech Stack

| Technology | Version |
|-----------|---------|
| .NET | 8.0 |
| Entity Framework Core | 8.0.0 |
| SQL Server LocalDb | - |
| JWT Bearer Authentication | 8.0.0 |
| Serilog | 8.0.0 |
| Swagger UI | 6.6.2 |

---

## Features

- Basic CRUD operations for Books (Create, Read, Update, Delete)
- JWT token generation using Basic Auth (Base64 encoded ClientId:ClientSecret)
- Serilog logging with hourly rolling log files stored by date
- Seed data (5 sample books auto inserted on first run)
- API versioning controlled from appsettings.json
- Swagger UI with JWT Bearer token support
- Response DTO with formatted CreatedAt (yyyy-MM-dd h:mm tt)
- AddedBy auto populated from JWT token ClientId

---

## Project Structure

```
LibraryManagementSystem/
├── Controllers/
│   ├── AuthController.cs        ← Token generation
│   └── BooksController.cs       ← CRUD endpoints
├── Data/
│   ├── AppDbContext.cs           ← EF Core DB context
│   └── SeedData.cs               ← Initial seed data
├── Models/
│   ├── Book.cs                   ← Book entity (DB model)
│   └── BookResponseDto.cs        ← API response model
├── Services/
│   └── TokenService.cs           ← JWT token generation
├── logs/                         ← Auto generated log files
│   └── yyyyMMdd/
│       └── logs-yyyyMMdd-HH.txt
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── LibraryManagementSystem.csproj
```

---

## Prerequisites

- Visual Studio 2022
- .NET 8.0 SDK — https://dotnet.microsoft.com/en-us/download/dotnet/8.0
- SQL Server LocalDb (included with Visual Studio 2022)

---

## NuGet Packages

The following NuGet packages are used in this project.

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.0 | JWT Bearer token authentication |
| Microsoft.EntityFrameworkCore | 8.0.0 | EF Core base package (DbContext, DbSet) |
| Microsoft.EntityFrameworkCore.Design | 8.0.0 | EF Core design-time support (EnsureCreated) |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.0 | SQL Server / LocalDb provider |
| Microsoft.EntityFrameworkCore.Tools | 8.0.0 | EF Core CLI tools (migrations) |
| Serilog.AspNetCore | 8.0.0 | Serilog integration with ASP.NET Core |
| Serilog.Sinks.File | 5.0.0 | Serilog file sink (write logs to file) |
| Swashbuckle.AspNetCore | 6.6.2 | Swagger UI for API documentation |
| System.IdentityModel.Tokens.Jwt | 7.6.3 | JWT token creation and validation |
```
```
### dotnet restore

```bash
dotnet restore
```

---

## SQL LocalDb Setup

### Step 1 — Create LocalDb Instance

Open **Command Prompt** or **PowerShell** and run:

```bash
# Create a new LocalDb instance named MyLocalDb
sqllocaldb create MyLocalDb

# Start the instance
sqllocaldb start MyLocalDb

# Verify instance is running
sqllocaldb info MyLocalDb
```

Expected output:
```
Name:               MyLocalDb
Version:            15.0.x.x
Shared name:
Owner:              DESKTOP\YourName
Auto-create:        No
State:              Running
Last start time:    01/07/2026 10:30:00
Instance pipe name: np:\\.\pipe\LOCALDB#XXXXX\tsql\query
```

---

### Step 4 — Verify Connection String in appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MyLocalDb;Database=LibraryDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

> The database `LibraryDb` will be **auto created** by the application
> on first run. You do not need to create it manually.

---

### Step 5 — Verify with SQL Server Object Explorer (Optional)

```
1. Open Visual Studio 2022
2. Go to View => SQL Server Object Explorer
3. Expand SQL Server
4. Look for (localdb)\MyLocalDb
5. After running the app, you should see LibraryDb under Databases
```

---

### Common LocalDb Commands

```bash
# List all LocalDb instances
sqllocaldb info

# Create instance
sqllocaldb create MyLocalDb

# Start instance
sqllocaldb start MyLocalDb

# Stop instance
sqllocaldb stop MyLocalDb

# Delete instance
sqllocaldb delete MyLocalDb

# Check version
sqllocaldb versions
```

---

## Getting Started

### Step 1 — Clone the repository

```bash
git clone https://github.com/Adamakmar-II/LibraryManagementSystem.git
cd LibraryManagementSystem
```

### Step 2 — Make sure LocalDb is running

```bash
sqllocaldb start MyLocalDb
```

### Step 3 — Restore packages

```bash
dotnet restore
```

### Step 4 — Run the application

```bash
dotnet run
```

> On first run:
> - Database `LibraryDb` is auto created
> - Books table is auto created
> - 5 sample books are auto seeded

### Step 5 — Open Swagger UI

```
http://localhost:port/swagger
```

---

## API Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `v1/api/Auth/token` | Generate JWT token | Basic Auth |
| GET | `v1/api/Books/search` | Get all books | Bearer Token |
| GET | `v1/api/Books/search?id=1` | Get book by Id | Bearer Token |
| POST | `v1/api/Books/create` | Create a book | Bearer Token |
| PUT | `v1/api/Books/update?id=1` | Update a book | Bearer Token |
| DELETE | `v1/api/Books/delete?id=1` | Delete a book | Bearer Token |

---

## How to Generate Token

### Step 1 — Encode credentials to Base64

```
ClientId     : userId1
ClientSecret : userSecret1
Combined     : userId1:userSecret1
Base64       : dXNlcklkMTp1c2VyU2VjcmV0MQ==
```

> You can encode online at: https://www.base64encode.org/
> Or use Postman built-in Base64 encoding.

### Step 2 — Call token endpoint in Postman

```
Method  : GET
URL     : http://localhost:port/v1/api/Auth/token
Headers :
  Key   : Authorization
  Value : Basic dXNlcklkMTp1c2VyU2VjcmV0MQ==
```

**Response:**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": "60 minutes"
}
```

### Step 3 — Use token for Book endpoints

```
Headers :
  Key   : Authorization
  Value : Bearer {your_token_here}
```

---

## Request Body (Create / Update)

```json
{
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "isbn": "978-0132350884",
  "publishedYear": 2008
}
```

> **Note:**
> - Do not include `addedBy` in the request body.
>   It is auto populated from the JWT token ClientId.
> - Do not include `createdAt` in the request body.
>   It is auto populated with the current datetime.

---

## Log Files

Log files are stored in the application root under `logs/` folder.
A new file is created every hour automatically.

```
logs/
└── 20241215/
    ├── logs-20241215-14.txt   ← 2:00 PM - 2:59 PM logs
    ├── logs-20241215-15.txt   ← 3:00 PM - 3:59 PM logs
    └── logs-20241215-16.txt   ← 4:00 PM - 4:59 PM logs
```

### Log format

```
2024-12-15 14:30:01 [INF] Token generated successfully for ClientId: libraryClient
2024-12-15 14:31:00 [INF] Search | Id: 1 | Title: Clean Code | Author: Robert C. Martin | ISBN: 978-0132350884 | PublishedYear: 2008 | AddedBy: system
2024-12-15 14:32:00 [INF] Create | Id: 6 | Title: Domain-Driven Design | Author: Eric Evans | ISBN: 978-0321125217 | PublishedYear: 2003 | AddedBy: libraryClient
2024-12-15 14:33:00 [INF] Update | Id: 1 | Title: Clean Code (2nd Edition) | Author: Robert C. Martin | ISBN: 978-0132350884 | PublishedYear: 2020
2024-12-15 14:34:00 [INF] Delete | Id: 5 | Title: Refactoring | Author: Martin Fowler
2024-12-15 14:35:00 [WRN] Search | Book not found | Id: 99
2024-12-15 14:36:00 [WRN] Update | Book not found | Id: 88
2024-12-15 14:37:00 [WRN] Delete | Book not found | Id: 77
```

---

## Database

### Connection

```
Server   : (localdb)\MyLocalDb
Database : LibraryDb
Auth     : Windows Authentication (Trusted Connection)
```

### Books Table

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key, auto increment |
| Title | nvarchar | Book title |
| Author | nvarchar | Book author |
| ISBN | nvarchar | ISBN number |
| PublishedYear | int | Year published |
| AddedBy | nvarchar | ClientId who added the book |
| CreatedAt | datetime2 | Full precision timestamp |

### Seed Data (Auto inserted on first run)

| Id | Title | Author | PublishedYear | AddedBy |
|----|-------|--------|---------------|---------|
| 1 | Clean Code | Robert C. Martin | 2008 | system |
| 2 | The Pragmatic Programmer | Andrew Hunt | 1999 | system |
| 3 | Design Patterns | Gang of Four | 1994 | system |
| 4 | Head First Design Patterns | Eric Freeman | 2004 | system |
| 5 | Refactoring | Martin Fowler | 1999 | system |

---

## Troubleshooting

```
# Problem: Cannot connect to LocalDb
# Fix: Make sure LocalDb instance is running
sqllocaldb start MyLocalDb

# Problem: Database not created
# Fix: EnsureCreated runs on startup automatically
#      Check connection string in appsettings.json

# Problem: Port already in use
# Fix: Change the port in launchSettings.json or run:
dotnet run --urls "http://localhost:5001"

# Problem: Token expired
# Fix: Generate a new token from v1/api/Auth/token
#      Default expiry is 60 minutes (configurable in appsettings.json)

# Problem: 401 Unauthorized on Book endpoints
# Fix: Make sure to include Bearer token in Authorization header
#      Authorization: Bearer {your_token_here}

# Problem: NuGet package restore failed
# Fix: Run the following commands
dotnet nuget locals all --clear
dotnet restore
```