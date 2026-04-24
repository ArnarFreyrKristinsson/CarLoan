# Car Loan Calculator

A .NET 8 car loan calculator applying SOLID and the Specification pattern. Building it up toward a MAUI mobile app that compares loan terms across Icelandic lenders.  Currently only has couple of validation rules such as minimum loan amount, maximum period, maximum ratio and more. 
Only calculates monthly payments at the moment. 

## What It Does

Validates loan applications against a set of independent business rules and calculates monthly payments when conditions are met.

**Validation rules:**

| Rule | Description |
|---|---|
| `MinimumLoanAmountValidator` | Loan amount must be at least 750,000 |
| `MinimumDownPaymentValidator` | Down payment must be at least 150,000 |
| `MinimumLoanPeriodValidator` | Loan period must be at least 6 months |
| `MaximumLoanPeriodValidator` | Enforces maximum loan ratio (90%), maximum period (84 months), and stricter limits for used cars (80% ratio / 72 months) |

**Calculation:**

`LoanCalculator` computes the monthly payment from a set of `LoanTerms` using the standard amortization formula.

## How It's Built

- **Specification Pattern** — each loan rule is its own class implementing `ILoanValidator` with an `Evaluate` method that returns a `LoanRuleResult`
- **Dependency Injection** — rules are registered in .NET's built-in DI container and injected as `IEnumerable<ILoanValidator>`, so the `LoanValidator` never knows which rules exist
- **TDD** — built test-first using xUnit with Osherove naming conventions (`MethodName_ExpectedResult_StateUnderTest`), following red-green-refactor
- **SOLID** — Open/Closed Principle in practice: new rules are new classes, existing code stays untouched

## Project Structure

```
CarLoan.Domain/            # Core domain library
├── Car.cs                 # Car record and CarCondition enum (New, Used)
├── Loan.cs                # Loan record composing LoanTerms and Car
├── LoanTerms.cs           # Immutable record holding all loan parameters
├── LoanRuleResult.cs      # Result record returned by each rule
├── ILoanValidator.cs       # Interface every rule implements
├── LoanValidator.cs        # Aggregates all ILoanValidator rules
├── LoanCalculator.cs       # Calculates monthly payment from LoanTerms
├── MinimumLoanAmountValidator.cs
├── MinimumDownPaymentValidator.cs
├── MinimumLoanPeriodValidator.cs
└── MaximumLoanPeriodValidator.cs

CarLoan.Domain.Tests/      # xUnit test project
├── LoanCalculatorTests.cs
├── LoanTermsTests.cs
└── LoanValidatorTests/
    ├── LoanValidatorIntegrationTests.cs
    ├── MinimumLoanAmountTests.cs
    ├── MinimumDownPaymentTests.cs
    ├── MinimumLoanPeriodTests.cs
    └── MaximumLoanPeriodTests.cs
```

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

The project demonstrates how to handle business logic that changes frequently. When a rule changes, you change one class. When a new rule is added, you add one class and register it. Nothing else moves. The test suite verifies each rule in isolation, so you know exactly what broke and why.
