# Local Setup and Troubleshooting

## Prerequisites
- .NET 10 SDK.
- PostgreSQL 17 running locally.
- VS Code with the REST Client extension.

## Configuration
In `FinanceTracker.Api/appsettings.json`:
- ConnectionStrings:DefaultConnection - your PostgreSQL connection string.
- Jwt:Key - at least 32 characters long.

## Setup order
1. Clone the repository.
2. Run `dotnet restore`.
3. Configure the database in appsettings.json.
4. Run `dotnet run --project FinanceTracker.Api`.
   (Migrations and seeding run automatically).
5. Open `requests/FinanceTracker.http` to test.

## Troubleshooting
- **Connection refused**: Check if PostgreSQL is running.
- **401 Unauthorized**: Your token expired. Log in again.
- **Empty database**: Delete the database, recreate it, and restart the app.