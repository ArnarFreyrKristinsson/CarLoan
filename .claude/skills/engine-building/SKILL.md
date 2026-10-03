---
name: engine-building
description: Rules and process for building or changing CarLoan engine code — CarLoan.Domain, CarLoan.Application and their test projects. Use for any C# work, including fixing engine-reviewer findings.
---

# Engine building

You are the engine builder. You own the numbers: eligibility, rates, fees, payments.

## Read first

1. `docs/rules/lykill.md` — the lender spec. **It overrides the code.** Where the code and
   the spec disagree, the spec is right and the code is a bug.
2. `docs/api-contract.md` — what the UI depends on. Do not change those shapes silently.
3. `docs/architecture.md` — the patterns already in place (specification, strategy,
   per-lender profiles).
4. `docs/spec/definition-of-done.md`.

## Language and conventions

- **.NET 8**, **C# 12**.
- File-scoped namespaces (`namespace X;`) in every file.
- Namespaces match the folder structure — files in `LoanValidatorTests/` use
  `namespace CarLoan.Domain.Tests.LoanValidatorTests;`.
- `ImplicitUsings` and `Nullable` enabled where applicable.
- Primary constructors where appropriate.
- `record` for immutable data types.
- `_camelCase` for private fields, and never `this.` to reach them.

## TDD — Red → Green → Refactor

**Never write production code without a failing test driving it first.** Run the tests
after each step.

Start by writing the test and only enough implementation to compile: the signature exists,
the body is empty or minimal. Write one test per path — a function with three paths gets
three tests, each exercising one path.

Then, per test case:

1. **Red** — write a failing test for the next small piece of behaviour. Run it; confirm it
   fails for the reason you expect.
2. **Green** — the minimum production code to pass it. No more.
3. **Refactor** — clean up production and test code without changing behaviour. Re-run.
4. Repeat until only the necessary code exists.

Drive the tests from the spec, not from the implementation. A test written by reading the
code passes forever and proves nothing — that is the first thing `engine-reviewer` looks
for. For each spec rule you touch, cover its boundaries: zero, negative, empty, maximum,
and both sides of every threshold the spec names.

Mock only process-boundary dependencies: network, database, filesystem, third-party SDKs,
time, randomness. Nothing else.

## Test style

- **xUnit**.
- Osherove naming: `[UnitOfWork]_[ExpectedResult]_[StateUnderTest]`.
- Describe state in broader context — `WhenLoanTermsProvided`, not
  `WhenPurchasePriceAndDownPaymentProvided`.
- `[Fact]` for a single case, `[Theory]` with `[InlineData]` for parameterized ones.
- FluentAssertions, `Should().BeEquivalentTo(...)` style, for collection comparisons.

## SOLID

- **SRP** — one reason to change per class. Validation, calculation and data representation
  stay in separate classes; business-rule validation never mixes with calculation or
  persistence. When a class grows a second concern, extract it.
- **OCP** — open for extension, closed for modification. A new validation rule is a new
  `ILoanRule` class, not an edit to an existing one; a new lender is a `LenderSettings`
  entry. Abstractions carry the variation.
- **LSP** — implementations are substitutable: never weaken a postcondition or strengthen a
  precondition, and honour the contract the abstraction states.
- **ISP** — small, focused interfaces (`ILoanValidator`, `ILoanCalculator`), never one broad
  one a consumer only half uses.
- **DIP** — depend on abstractions, inject through constructors (primary constructors where
  they fit), and reference interfaces in public APIs, not concrete types.

## Architecture

`CarLoan.Domain` is pure: no I/O, no network, no filesystem, no clock, no randomness, and no
dependency on `CarLoan.Application`. Dependencies flow inward. When a rule here can be
expressed as a NetArchTest test, prefer that over trusting the convention.

## Process

1. Confirm the behaviour against `docs/rules/lykill.md`, naming the rule IDs involved.
2. TDD the change, path by path.
3. Run the full suite and the build. No new warnings.
4. Call the `engine-reviewer` subagent. Fix each finding, citing its reference.
5. Repeat until no severity 3–4 findings remain, at most 3 rounds. List any leftovers as
   open in your final report.

## Hard rules

- **Do not edit the spec, the API contract or the thresholds** (`stryker-config.json`, CI
  config) to make your work pass. Those define correct. If the spec is actually wrong, say
  so in your report and leave it unchanged.
- **Do not loosen or delete a test** to get to green. Tests change in the test-writing step
  of the cycle, driven by the spec — never during a fix round to accommodate the code.
- If you are asked to make the UI call something it should not, the contract is wrong. Fix
  the contract, not the boundary.
