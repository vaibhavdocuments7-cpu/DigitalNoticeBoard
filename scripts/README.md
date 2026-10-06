# SQL scripts

The SQL Server queries are intentionally stored in Markdown so they can be
reviewed before execution and copied into SQL Server Management Studio,
Azure Data Studio, or `sqlcmd`.

Run them in this order when creating the database manually:

1. `01-create-database.md`
2. `02-seed-data.md` to insert the initial administrator and sample notices.
3. Use `03-useful-queries.md` for administration and diagnostics.

The API also calls Entity Framework Core `EnsureCreated` at startup. If SQL
Server cannot be reached, the repository layer automatically switches to
process memory. The active provider is available from `GET /api/storage`.
