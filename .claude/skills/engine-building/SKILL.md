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

Apply these as general design principles to all code you write, including code with no
precedent in this repo yet. Each principle is stated in general terms first; the
*Example* after it shows how it already looks in this codebase and is an illustration, not
the limit of the rule.

- **SRP, Single Responsibility.** A class has one reason to change: it serves one concern.
  When a class starts serving a second concern, extract it.
  *Example:* validation, calculation and data representation live in separate classes.
- **OCP, Open/Closed.** Add behaviour by adding code, not by editing code that already
  works. Put the thing that varies behind an abstraction, so a new case is a new
  implementation or new configuration.
  *Example:* a new validation rule is a new `ILoanRule` class; a new lender is a new
  `LenderSettings` entry.
- **LSP, Liskov Substitution.** Every implementation must work wherever its abstraction is
  expected, without the caller knowing which one it got. Never demand more of the inputs
  (a stronger precondition), promise less about the result (a weaker postcondition), or
  throw where the abstraction does not.
  *Example:* any `ILoanRule` can go into a lender's rule list, and `LoanValidator` runs it
  without knowing which rule it is.
- **ISP, Interface Segregation.** Keep interfaces small and shaped by what one kind of
  consumer needs. No consumer should depend on members it does not use; split a broad
  interface instead.
  *Example:* `ILoanValidator` and `ILoanCalculator` are separate interfaces.
- **DIP, Dependency Inversion.** High-level policy depends on abstractions, not on concrete
  details; the details implement those abstractions. Inject dependencies through
  constructors (primary constructors where they fit), and expose interfaces, not concrete
  types, in public APIs.
  *Example:* `MultiLenderLoanApplicationService` receives an `ILoanCalculator`.

## Architecture

`CarLoan.Domain` is pure: no I/O, no network, no filesystem, no clock, no randomness, and no
dependency on `CarLoan.Application`. Dependencies flow inward. When a rule here can be
expressed as a NetArchTest test, prefer that over trusting the convention.

## Process

1. Confirm the behaviour against `docs/rules/lykill.md`, naming the rule IDs involved.
2. TDD the change, path by path.
3. Run the full suite and the build. No warnings, except a justified suppression (see
   `docs/spec/definition-of-done.md`).
4. Call the `engine-reviewer` subagent. Fix each finding, citing its reference.
5. Repeat until the reviewer reports no findings, at most 3 rounds. End with the result:
   `No findings`, or every finding that remains, most severe first.

## Hard rules

- **Do not edit the spec, the API contract or the thresholds** (`stryker-config.json`, CI
  config) to make your work pass. Those define correct. If the spec is actually wrong, say
  so in your report and leave it unchanged.
- **Do not loosen or delete a test** to get to green. Tests change in the test-writing step
  of the cycle, driven by the spec — never during a fix round to accommodate the code.
- If you are asked to make the UI call something it should not, the contract is wrong. Fix
  the contract, not the boundary.
