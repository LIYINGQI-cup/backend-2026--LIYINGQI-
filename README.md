# Finance Tracker API - Backend Development 2026

A RESTful API for a personal finance tracker, implemented with ASP.NET Core 10, PostgreSQL, and Entity Framework Core. This is Project 1 (core version) for the Backend Development 2026 course.

## Tech Stack
- Framework: ASP.NET Core 10 Web API
- Database: PostgreSQL (via Npgsql)
- ORM: Entity Framework Core
- Authentication: ASP.NET Core Identity + JWT (HS256)
- Testing: xUnit with EF Core InMemory

## Prerequisites
- .NET 10 SDK (https://dotnet.microsoft.com/download/dotnet/10.0)
- PostgreSQL 17 (https://www.postgresql.org/download/) running on localhost:5432

## Quick Start

### 1. Clone the repository
git clone https://github.com/LIYINGQI-cup/backend-2026--LIYINGQI-.git
cd backend-2026--LIYINGQI-

### 2. Configure the database
Create a database named fintracker in PostgreSQL:
CREATE DATABASE fintracker;

### 3. Set up configuration
Open FinanceTracker.Api/appsettings.json and update the following:
- ConnectionStrings:DefaultConnection: Replace with your PostgreSQL credentials.
- Jwt:Key: A secret key at least 32 characters long.

### 4. Run the application
dotnet run --project FinanceTracker.Api

The application will automatically apply migrations and seed the database on the first run.

### 5. Test the API
Open requests/FinanceTracker.http (or any .http file in the requests/ folder) in VS Code with the REST Client extension. Log in first to get your accessToken, then test the endpoints.

## Demo User
- Email: demo@fintracker.local
- Password: Demo12345

## Implemented Features (Project 1)
- Authentication & Profile: JWT login/register, profile GET/PUT.
- Accounts: Full CRUD, archiving, and derived balance calculation (R1).
- Categories: System and user categories with uniqueness rules (R10-R12).
- Operations: Full CRUD with filters, sorting, and paging (R2-R5).
- Transfers: Same-currency transfers between own accounts (R8).
- Statistics: Totals, byCategory shares, and timeline grouped by day/month (R16).

## Bonuses
- Unit Tests: xUnit test project covering authentication and operations validation (R4, R5).
- .http Test Files: A complete set of requests for every endpoint, including negative test cases.

## Project Structure
- FinanceTracker.Api: Controllers, DTOs, Program.cs.
- FinanceTracker.Infrastructure: DbContext, Migrations, SeedData, Identity/JWT.
- FinanceTracker.Domain: Domain entities and enums.
- FinanceTracker.Tests: Unit tests.
- requests/: .http files for testing.

