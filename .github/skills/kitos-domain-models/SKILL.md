---
name: kitos-domain-models
description: "Implement or review KITOS domain models and domain-field changes. Use when adding or changing entities, properties, constructors, aggregate creation/update methods, or propagating domain fields through application services, APIs, and EF Core."
---

# KITOS domain models

## Trace ownership before editing

Read the entity, its aggregate/root, constructors and factory methods, and the
existing mutation helpers. Find every caller of the affected creation and update
methods, including migrations between systems, event handlers, cleanup, imports,
and tests. Read neighboring implementations before choosing a new pattern.

Trace the complete field path:

`Request DTO -> write mapper -> application parameters -> application service -> domain creation/update method -> entity -> EF Core`

Also trace response mappers, shared response DTOs, and relevant read models.
Do not stop at adding an entity property or an assignment in an application service.

## Keep creation and mutation in the domain

- Extend the existing domain creation method when adding a field to an entity it
  creates. The returned entity must already contain the supplied value.
- For example, a relation direction belongs in the input to
  `ItSystemUsage.AddUsageRelationTo`, which initializes the `SystemRelation`.
  Do not call the factory and then patch the returned entity in an application
  service's success callback.
- Extend the existing domain update method and its shared mutation helper where
  applicable. Do not bypass that path with an application-service property
  assignment just because the setter is public.
- Keep business invariants in the domain. Return `Result<T, OperationError>` or
  `Maybe<T>` according to existing conventions for expected business failures.
  Preserve existing validation and failure behavior.
- Application services authorize, resolve identities, orchestrate domain calls,
  persist, and raise events. Domain entities must not depend on controllers,
  API DTOs, repositories, transaction managers, or event publishers.
- A descriptive field does not imply new relationship, visibility, authorization,
  interface-exposure, or other data restrictions. Add such rules only when the
  requirement calls for them.

Avoid unrelated encapsulation refactors or abstractions. Follow existing KITOS
patterns rather than making all setters private as part of a field addition.

## Define null and update semantics explicitly

- Use nullable entity/API properties when absence is a valid state; do not invent
  an `Unspecified` enum member or populate historical rows with an arbitrary value.
  Use `Maybe<T>` where it is the established optional-value contract.
- Distinguish "omitted/unchanged", "clear", and "set" when the endpoint needs those
  operations. Reuse existing `OptionalValueChange<T>`/PATCH mapping patterns.
  A nullable parameter alone cannot distinguish omission from explicit null.
- Check PUT, PATCH, and internal callers separately. A cleanup handler changing
  an interface or contract must not erase unrelated relation metadata.
- Do not add an optional parameter merely to keep callers compiling. Inspect
  every caller and deliberately preserve or replace the existing value.
- Ask for a decision if null/omission semantics or a backwards-compatible default
  are genuinely ambiguous; do not silently pick a behavior.

## Propagate the contract and persistence

- Keep controllers thin and mapping in the existing write/response mappers.
  Follow the API's separate enum `Choice` and `EnumMap` conventions.
- Update shared DTOs when multiple response shapes expose the same field.
  Keep direction metadata separate from the source/target identity of a relation.
- Match nullability across DTOs, parameters, domain methods, entity, EF mapping,
  migration, designer, and snapshot. Review defaults and historical data behavior.
- Use PostgreSQL with EF Core only. Scaffold actual schema changes under
  `Infrastructure.DataAccess\Migrations\EfCore`, including the designer and
  snapshot; inspect generated changes for unrelated model drift.
- Do not apply migrations to a database unless explicitly requested.
- Update read models and their maintenance paths only when the field is required
  there. Reuse existing authorization, transaction, and event behavior; consult
  `kitos-transactions` or `kitos-pubsub-events` when changing those concerns.

## Verify the domain contract directly

- Add direct domain tests: the creation method returns an entity with the value
  already populated, and the update method sets or clears it as specified.
- Cover nullable values and each enum choice where relevant. Assert literal
  expected values rather than using the production mapper to compute expectations.
- Verify existing invariants still hold and descriptive metadata adds no new
  restrictions. Cover failure paths affected by the change.
- Test that cleanup, migration, and other partial-update callers preserve unrelated
  fields. Compilation alone does not verify this.
- Add focused application-service and mapper tests for forwarding and responses,
  plus API integration coverage for contract changes. Do not rely solely on a
  service test that could pass because the service repairs an incomplete entity.
- Use `WithAutoFixture`, Moq, and existing test naming conventions. Run focused
  tests and the full `pwsh .\scripts\quality-check.ps1` gate with tests enabled.
  Self-review with `code-quality`; report blocked validation plainly.
