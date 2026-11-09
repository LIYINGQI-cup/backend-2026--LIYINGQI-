# Decision Record

## D1: Balance is derived, never stored (R1)
- **Context**: Storing balance can drift from actual operations.
- **Decision**: Calculate balance on every read.
- **Alternatives**: Stored column (rejected due to data inconsistency risk).
- **Consequences**: Reads are slightly more expensive but always correct.

## D2: Three-project layer separation
- **Context**: Project 2 needs extensibility.
- **Decision**: Api → Infrastructure → Domain.
- **Consequences**: Clear dependency direction, testable services.

## D3: Seed only on an empty database
- **Context**: Seeding on every startup duplicates data.
- **Decision**: SeedData checks if any user exists before inserting.
- **Consequences**: Re-seeding requires dropping the database.