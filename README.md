# CarLoan

[![CI](https://github.com/ArnarFreyrKristinsson/CarLoan/actions/workflows/ci.yml/badge.svg)](https://github.com/ArnarFreyrKristinsson/CarLoan/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/ArnarFreyrKristinsson/CarLoan/branch/master/graph/badge.svg)](https://codecov.io/gh/ArnarFreyrKristinsson/CarLoan)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FArnarFreyrKristinsson%2FCarLoan%2Fmaster)](https://dashboard.stryker-mutator.io/reports/github.com/ArnarFreyrKristinsson/CarLoan/master)

A .NET 8 loan comparison engine for car loans. Give it a loan request and it evaluates that request 
against every lender it knows about, returning each lender's answer: the interest rate, the origination fee, 
the monthly payment, and when a lender would decline, exactly which rules failed and why.

A front end is planned but today the engine's only consumer is its test suite.

## Status

| Piece | State |
|---|---|
| Engine | Working end to end, validated against a published rule spec |
| Lenders | **1 — Lykill.** More will be added; the engine fans out over however many are registered |
| Comparison / ranking layer | Not written yet — results come back one per lender, unranked |
| UI | Not built (Blazor planned) |

## How a request flows

1. `LoanRequest` → `LoanRequestMapper` → a domain `Loan` (pre-fee amount, LTV, car).
2. For each registered `LenderProfile`: look up the interest rate, compute the origination fee, 
   then run that lender's rules against the priced loan.
3. Return a `LenderLoanEvaluationResult` per lender: rate, fee, monthly payment, and every rule result.

Each `LoanRuleResult` carries the rule name, a human-readable message, and a parameter dictionary, 
so a consumer can show why a lender said no rather than just that it did.

## Lenders

Each lender's rules and pricing come from a spec in [`docs/rules/`](docs/rules/), and every rule in a spec has a an ID. 
The spec is the source of truth.

| Lender | Spec |
|---|---|
| Lykill | [docs/rules/lykill.md](docs/rules/lykill.md) |

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

## Architecture

Design patterns, the per-lender registry, and testing practice: [docs/architecture.md](docs/architecture.md).
