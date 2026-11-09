# Architecture and Data Model

## Component structure
- `FinanceTracker.Api`: controllers, Program.cs, configuration, DTOs.
- `FinanceTracker.Infrastructure`: DbContext, EF configurations, migrations, SeedData, Identity/JWT.
- `FinanceTracker.Domain`: entities, enums.

References go one way only: Api → Infrastructure → Domain.

## Request flow
1. HTTP request → controller (DTO in, DTO out).
2. Controller calls the service or DbContext directly.
3. Business rules are validated.
4. Errors are mapped to ProblemDetails.

## Entities and invariants
- **User (ApplicationUser)**: Identity user with Guid keys.
- **Account**: Name, Currency, StartAmount, IsArchived. Balance is calculated on read (R1).
- **Category**: UserId null for system categories. (UserId, Name, Type) is unique.
- **Operation**: Type Income/Expense, Amount > 0, Date cannot be in the future.
- **Transfer**: Two different accounts, same currency, same user.

## Database specifics
- Money: numeric(18,2). Dates: date.
- Database.Migrate() runs at startup; SeedData runs only on an empty database.