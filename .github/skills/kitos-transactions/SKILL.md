---
name: kitos-transactions
description: "Implement or review KITOS application-service transactions. Use for transaction boundaries, nested service calls, commit/rollback behavior, and event ordering."
---

# KITOS transactions

## Start with existing behavior

Read `Infrastructure.Services/DataAccess/ITransactionManager.cs`,
`IDatabaseTransaction.cs`, and the implementations under
`Infrastructure.DataAccess/Services`. Trace callers before moving boundaries,
especially archiving and cascading deletion.

- Inject `ITransactionManager` and use `using var transaction = _transactionManager.Begin()`.
- Preserve authorization before mutation and existing `Result`/`Maybe` error paths.
- Persist through the established repository/`IDatabaseControl` pattern; do not
  add direct `DbContext.SaveChanges()` calls to application services.
- Call `Commit()` only on success. Follow the surrounding rollback/error pattern;
  disposal of an uncommitted owning database transaction rolls it back.

## Keep the abstraction simple

`TransactionManager.Begin()` starts a database transaction only when the context
has none. Otherwise it returns `WithinAmbienTransaction`, whose `Commit()`,
`Rollback()`, and `Dispose()` are no-ops. The outer owner decides the database
outcome; a nested rollback does not undo changes or mark the owner rollback-only.
Propagate nested failures so the outer owner does not commit failed work.

For usage PubSub events, capture UUIDs before cleanup, complete persistence, call
`transaction.Commit()`, then raise the event from the application service. Keep
generic pre-deletion cleanup events before removal.

Do not introduce callback queues, scope tracking, or changes to shared transaction
interfaces solely for event publication. Prefer the existing service-level
commit-then-event pattern.

Code after a nested `Commit()` is not necessarily after the database commit.
Best-effort notifications can therefore precede an enclosing rollback. State
this limitation explicitly. If a requirement needs outermost-commit or durable
delivery, agree on the broader design before implementing it; do not imply that
the simple pattern provides those guarantees.

## Validate the boundary

- Test success, authorization/not-found failures, persistence failures, and commit
  exceptions. Failures before commit must not emit the post-commit event.
- Verify ordering: persistence before `Commit()`, then exactly one event with
  captured identifiers. Keep existing cleanup events and transaction behavior.
- Exercise relevant archive/cascade paths and error propagation when changing
  boundaries. Use PostgreSQL for database-level tests, never provider variants.
- Run focused tests and `pwsh .\scripts\quality-check.ps1` with tests enabled;
  self-review using the `code-quality` skill.
