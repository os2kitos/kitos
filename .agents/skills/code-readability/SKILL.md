---
name: code-readability
description: Apply the user's method-extraction and no-magic-numbers preferences when writing, modifying, or refactoring code. Extract cohesive multi-statement operations into named methods so callers show intent and remain easy to read. Use for implementation and readability reviews, not as a general security, performance, or architecture audit.
---

# Code readability

When writing or modifying code, always extract a cohesive multi-statement block that performs a distinct operation into a clearly named method, even when it is used only once. Readability is sufficient reason for extraction; duplicated code is not required.

Keep the calling method focused on the sequence of meaningful operations. Choose names that explain the operation, such as `MapInvoiceDates`, and keep its related calculations and assignments together. Follow the surrounding naming and visibility conventions; use a private static helper when appropriate and no instance state is required.

For example, ordering invoice dates, projecting them into a read-model collection, looking up individual periods, and assigning their month/day fields form one operation. Put that block in `MapInvoiceDates(source, destination)` rather than leaving it inline among other mappings.

Choose boundaries by responsibility rather than an arbitrary line count. Do not split each assignment into its own method or wrap a simple expression when that adds indirection without clarifying intent. Apply the preference to code touched by the task; do not expand a small change into an unrelated repository-wide refactor.

During extraction, preserve execution order, null handling, enumeration behavior, exceptions, and side effects. Check the resulting diff and run relevant existing tests when appropriate. Do not add tests solely to assert that a helper method exists.

Avoid magic numbers in code. Replace numeric literals that encode domain meaning, limits, or rules with clearly named constants or existing domain enums. Reuse established definitions and choose the narrowest useful scope. For example, use FirstInvoicePeriod instead of a bare 1 when selecting an invoice period. Keep obvious zero initializations, indexes, and simple cardinality checks literal when a name would add no meaning. Preserve values and behavior, and apply this guidance only within the requested scope.
