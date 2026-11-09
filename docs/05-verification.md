# Verification and Release Checklist

## Verification steps
- [x] App starts on an empty database and applies migrations.
- [x] Demo user `demo@fintracker.local` can log in.
- [x] Accounts return exact derived balances (12710.00, 48928.50, 2055.00).
- [x] Categories enforce R10-R12 rules.
- [x] Operations enforce R2-R5 rules.
- [x] Transfers enforce R8 rules.
- [x] Statistics match the exact seed data totals (60000.00 / 53370.75).

## Evidence
- Unit tests (`dotnet test`) pass.
- `.http` requests return expected status codes (201, 400, 409).