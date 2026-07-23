---
applyTo: "src/**/*.cs,tests/**/*.cs"
---
# Test-Driven Development (mandatory)

Work strictly in **Red-Green-Refactor** cycles. Tests come first, implementation follows.

1. **RED** — Write the failing tests that describe the desired behavior first.
   Do not write production code before the tests exist.
2. **GREEN** — Write the minimum implementation needed to make the tests pass.
3. **REFACTOR** — Clean up without changing behavior; tests stay green.

## Rules

- No new public type or member without a test written **before** it.
- Cover **both success and failure cases**. Derive the cases from the acceptance
  criteria: expected return values, state changes, thrown exceptions, and boundary
  or edge conditions (null, empty, invalid arguments, cancellation).
- Test the public API surface and observable behavior, not private implementation
  details.
- Show the failing (red) test state before implementing.
- The build and the full test suite must be green before the work is considered done.
- Do not weaken or delete a test just to make it pass; if a test is wrong, explain why.

## Feature workflow

When implementing a new public type or member, follow this order:

1. Confirm the **contract** (signature, inputs, outputs, error behavior) from the
   ticket and any applicable design docs or ADRs.
2. **RED:** write the tests for every acceptance criterion and run them
   (they must fail).
3. **GREEN:** implement the minimum production code until the tests pass.
4. **REFACTOR:** tidy up while keeping tests green.

The human reviews the **tests** first (the control point), then the implementation is
verified objectively by the tests passing.
