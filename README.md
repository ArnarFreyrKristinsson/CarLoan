# Car Loan Calculator

A .NET 8 car loan calculator applying the SOLID principles where applicable and possible more design patterns or principles learned in the process. This includes validations and calculations. Currently only has couple of validation rules such as minimum loan amount, maximum period, maximum ratio and more. 
Only calculates monthly payments at the moment. The project will also use Blazor in the future. 

## What It Does

Validates loan applications against a set of independent business rules and calculates monthly payments when conditions are met.

## Scope

The current stage covers the happy path: inputs are assumed to be structurally valid (positive prices, positive periods, sensible rates). Behavior outside of that is undefined.

**Validation rules:**

All rules are configured per lender; the values below are the ones wired up for Lykill, per [docs/rules/lykill.md](docs/rules/lykill.md).

| Rule | Description |
|---|---|
| `MinimumLoanAmountValidator` | Loan amount must be at least 750,000 |
| `MaximumLoanAmountValidator` | Loan amount must not exceed 30,000,000 |
| `MinimumDownPaymentValidator` | Down payment must be at least 150,000 |
| `MinimumLoanPeriodValidator` | Loan period must be at least 1 month |
| `MaximumLoanPeriodValidator` | Enforces maximum loan ratio (90%), maximum period (84 months), and stricter limits for used cars above 80% ratio (72 months) |
| `CarAgeValidator` | Used cars only: car age plus loan term must not exceed 12 years above 80% ratio, 20 years at or below it |

**Pricing:**

Interest rates are keyed on the financing ratio (LTV), with separate general and green (electric/hydrogen) rate tables. An origination fee is charged on the loan amount by contract length, with a discount for green vehicles (off the fee amount) and plug-in hybrids (off the fee rate), an 18,000 minimum, and the result financed on top of the loan.

**Calculation:**

`LoanCalculator` computes the monthly payment from a set of `LoanTerms` using the standard amortization formula.

## How It's Built

- **TDD** — built test-first using xUnit with Osherove naming conventions (`MethodName_ExpectedResult_StateUnderTest`), following red-green-refactor
- **SOLID** — Is followed where applicable for example: Open/Closed Principle in practice - new rules are new classes, existing code stays untouched.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Getting Started

```bash
# Clone the repository
git clone https://github.com/ArnarFreyrKristinsson/CarLoan.git
cd CarLoan

# Build
dotnet build

# Run tests
dotnet test
```

## Why This Architecture

So far the project demonstrates how to handle business logic that changes frequently. When a rule changes, you change one class. When a new rule is added, you add one class. Nothing else moves. The test suite verifies each rule in isolation, so you know exactly what broke and why.
