# Copilot Instructions

## General

- Target **.NET 8** and **C# 12**.
- Use **file-scoped namespaces** (`namespace X;`) in all files.
- Namespaces must match the folder structure (e.g., files in `LoanValidatorTests/` use `namespace CarLoan.Domain.Tests.LoanValidatorTests;`).
- Enable `ImplicitUsings` and `Nullable` where applicable.
- Use **primary constructors** where appropriate (C# 12).
- Use **records** for immutable data types.

## Naming Conventions

- Use **_camelCase** (underscore prefix) for private fields. Do not use `this.` to reference them.

## Testing

- Use **xUnit** as the test framework.
- Test method names should follow the **Osherove naming convention**: `[UnitOfWork]_[ExpectedResult]_[StateUnderTest]`. 
- When describing state, refer to the broader context (e.g., "WhenLoanTermsProvided" instead of listing specific fields 
  like "WhenPurchasePriceAndDownPaymentProvided").
- Use `[Fact]` for single-case tests and `[Theory]` with `[InlineData]` for parameterized tests.

## SOLID Principles

All code in the project must follow the **SOLID** principles:

### Single Responsibility Principle (SRP)
- Each class must have **one reason to change**. Keep validation, calculation, and data representation in separate classes.
- Do not mix business rule validation with loan calculation logic or data persistence concerns.
- When a class starts handling multiple concerns, extract the additional responsibility into its own class.

### Open/Closed Principle (OCP)
- Classes should be **open for extension but closed for modification**.
- Favour adding new classes or implementations over modifying existing ones when introducing new behaviour (e.g., a new validation rule or calculation strategy).
- Use **interfaces or abstractions** so new variants (e.g., different car conditions, loan products) can be supported without changing existing code.

### Liskov Substitution Principle (LSP)
- Derived types must be **substitutable** for their base types without altering correctness.
- Do not override methods in a way that weakens postconditions or strengthens preconditions.
- When using inheritance or interface implementations, ensure all implementations honour the contract defined by the abstraction.

### Interface Segregation Principle (ISP)
- Keep interfaces **small and focused**. A consumer should not be forced to depend on methods it does not use.
- Prefer multiple fine-grained interfaces (e.g., `ILoanValidator`, `ILoanCalculator`) over a single broad interface.

### Dependency Inversion Principle (DIP)
- High-level modules must **not depend on low-level modules**; both should depend on abstractions.
- Accept dependencies through **constructor injection** (leveraging primary constructors where appropriate).
- Reference **interfaces or abstract types** rather than concrete implementations in public APIs and constructors.