# Architecture

How the engine is put together, and why.

## Specification pattern

Every validation rule is a class behind `ILoanRule`, evaluated independently and reported
individually — see `CarLoan.Domain/Validators/` (`MinimumLoanAmountValidator`,
`MaximumLoanPeriodValidator`, `CarAgeValidator`, etc.). A `LoanValidator` runs a lender's full
rule set and collects every result, not just the first failure, so a caller can show all the
reasons a loan was declined at once.

This gives Open/Closed in practice: a new rule is a new class implementing `ILoanRule`. Existing
rules, the validator, and every other lender's rule set stay untouched. The test suite verifies
each rule in isolation, so when one breaks you know exactly which rule and why.

## Strategy pattern

Rate lookup, fee calculation, and payment calculation are each behind an interface —
`ILoanInterestRateProvider`, `IOriginationFeeCalculator`, `ILoanCalculator` — so a lender can swap
in its own pricing behavior without touching the engine that drives it.

## Data-driven parameters

Rate bands and fee tiers are sorted data (`RateTier`, `FeeTier`), not conditionals. Looking up a
rate or fee tier is a table scan against these records, not a chain of `if`/`else`. A new band or
a changed boundary is a new record or an edited number, not new branching logic.

## Per-lender policy objects

A `LenderProfile` (`CarLoan.Domain/Lenders/LenderProfile.cs`) bundles one lender's name, rule set,
rate provider, and fee calculator into a single record. `LenderProfiles.Build()`
(`CarLoan.Application/LenderProfiles.cs`) is the registry: it builds each lender's `LenderProfile`
from a `LenderSettings` record and returns them keyed by name. The engine fans out over whatever
is in that registry — adding a lender is adding one `LenderSettings` entry and, if its rule set
differs from the standard one, wiring that up in `Build()`. Nothing about the engine itself
changes.

## Testing

Built test-first with xUnit, using Osherove naming
(`MethodName_ExpectedResult_StateUnderTest`). Mutation-tested weekly with Stryker to catch
assertions that pass without actually exercising the behavior they claim to check.
