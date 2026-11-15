# API Contract

The one place the UI and the engine meet. Both builders read this file instead of guessing
what the other side does, and either reviewer can check an implementation against it.

**Status: defined** (decisions settled 2026-10-03; amended 2026-11-02 for the calculator
stories: `LoanFigures`, nullable prices, `PricingGaps`, `MaximumLoanPeriodInMonths`).

## Transport

The UI is a Blazor Web App rendered on the server: static SSR by default, with individual
components opted into Interactive Server only where a story needs live updates. Either way
the UI calls the engine **in-process** through the interface below. There is no HTTP
endpoint and no JSON wire format; the C# types are the contract. If a WebAssembly client
or a public API is ever added, this section gains a route, wire names and contract tests
first.

## Engine entry point

`CarLoan.Application.IMultiLenderLoanApplicationService`

```csharp
LoanEvaluationOutcome EvaluateLoanRequest(LoanRequest request);
```

`LoanEvaluationOutcome` is closed — exactly one of:

| Outcome | Meaning | Carries |
|---|---|---|
| `LoanEvaluationOutcome.InvalidRequest` | The request is malformed; no lender evaluated it | `Errors`: `IReadOnlyList<InputError>` |
| `LoanEvaluationOutcome.Evaluated` | Every lender evaluated it — accepted or declined | `Lenders`: one `LenderLoanEvaluationResult` per lender; `Loan`: the `LoanFigures` that are the same for every lender |

These are different things and the UI tells them apart by type, never by catching an
exception (N9). A `null` request is a programming error and still throws.

Engine configuration built with an invalid value (a lender profile, a rate table, a fee
schedule) is a programming error too, and the guards in `CarLoan.Domain/Guards` throw for
it. The exception a guard throws names the parameter the guard was given: its `ParamName`
is part of the behaviour, its message wording is not.

### Request — `LoanRequest`

| Field | Type | Notes |
|---|---|---|
| `PurchasePrice` | decimal | kr, pre-fee |
| `DownPayment` | decimal | kr, the buyer's own contribution |
| `LoanPeriodInMonths` | int | months, even though the eligibility rules in `docs/rules/lykill.md` are stated in years |
| `CarCondition` | `RequestedCarCondition` | enum |
| `VehicleCategory` | `RequestedVehicleCategory` | enum, V1–V3 in the spec |
| `CarAgeInYears` | int | |

All amounts are `decimal` kr, with one exception: the limit
`LoanRequestValidator.MaximumPurchasePrice` is a `long` in whole kr (see **Input errors**).
Loan amount is `PurchasePrice - DownPayment` and excludes fees. The UI does not compute it:
it reads `LoanFigures.LoanAmount` — see **Division of responsibility**.

### Response — `LoanFigures`, once per request

| Field | Type | Notes |
|---|---|---|
| `LoanAmount` | decimal | kr, purchase price minus down payment, excludes fees |
| `FinancingRatioRoundedUp` | decimal | percent, one decimal, always rounded **up**, as a screen shows it: `80.03` → `80.1`, `79.96` → `80.0`, so it never appears on the wrong side of a limit |
| `DownPaymentPercentage` | decimal | percent, `100 − FinancingRatioRoundedUp` |

The rules and rates use the exact ratio, never the rounded one.

### Response — one `LenderLoanEvaluationResult` per lender

| Field | Type | Notes |
|---|---|---|
| `LenderName` | string | |
| `ValidationResults` | `IReadOnlyList<LoanRuleResult>` | every rule's result, not just failures, so the UI can show all reasons a loan was declined at once — see below |
| `IsAccepted` | bool | every rule passed |
| `MonthlyPayment` | decimal? | kr, rounded to 2 decimals as in `lykill.md` §7; null when `PricingGaps` is not empty |
| `InterestRate` | decimal? | **percent**: `10.35` means 10.35 %, not `0.1035`; null when the lender has no rate for the financing ratio |
| `OriginationFee` | `OriginationFee?` | null when the lender has no fee for the term |
| `FinancedAmount` | decimal? | kr, loan amount plus the fee; null with `OriginationFee` |
| `TotalInterest` | decimal? | kr, the monthly payment × the term in months, minus the financed amount; null with `MonthlyPayment` |
| `PricingGaps` | `IReadOnlyList<PricingGap>` | why a price is missing; empty when the lender prices the loan |
| `MaximumLoanPeriodInMonths` | int? | the longest term the lender's term and car-age rules allow for this car and financing ratio, `0` when no term is; null when no rule limits the term |

A declined loan is still priced wherever the lender's schedule has a price for it. A null
price means the schedule has none: there is no number to show, and the UI shows none.

`OriginationFee` carries `Amount` (kr, after every discount and the minimum), `AmountSaved`
(kr, F6), `EffectiveRate` (percent, the rate as it applies: a green discount is shown as the
halved rate) and `MinimumFeeApplied` (the minimum fee raised the fee).

### Pricing gaps — `PricingGap(Code, Minimum, Maximum)`

The range the lender's schedule prices, reported when the loan falls outside it.

| `Code` | Range | Meaning |
|---|---|---|
| `InterestRate` | financing ratio in percent, e.g. `0`–`90` | no rate band for this financing ratio, so no rate and no payment |
| `OriginationFee` | term in months, e.g. `6`–`84` | no fee band for this term, so no fee, no financed amount and no payment |

Both can be reported at once.

### Input errors — `InputError(Field, Kind)`

| Field | Type | Notes |
|---|---|---|
| `Field` | string | the `LoanRequest` property name, e.g. `nameof(LoanRequest.PurchasePrice)` |
| `Kind` | `InputErrorKind` | see below |

| `Kind` | When |
|---|---|
| `MustBePositive` | `PurchasePrice` or `LoanPeriodInMonths` is zero or negative |
| `MustNotBeNegative` | `DownPayment` or `CarAgeInYears` is negative |
| `MustBeLessThanPurchasePrice` | `DownPayment` ≥ `PurchasePrice` — nothing left to finance (only checked when `PurchasePrice` is positive, so one mistake gives one error) |
| `TooLarge` | `PurchasePrice` above `LoanRequestValidator.MaximumPurchasePrice` (10¹² kr) or `LoanPeriodInMonths` above `LoanRequestValidator.MaximumLoanPeriodInMonths` (1,200) |
| `Unsupported` | `CarCondition` or `VehicleCategory` is not a defined enum value |

Every malformed field is reported at once, so the UI can mark them all next to their
inputs. The UI's own form checks stop at "filled in" and "is a number"; ranges are the
engine's. The UI may read the two `Maximum…` constants for its input limits (N5) rather
than repeating the numbers. Both are compile-time constants: `MaximumPurchasePrice` is a
`long` in whole kr, which converts implicitly to the `decimal` it is compared with, and
`MaximumLoanPeriodInMonths` is an `int`. The bounds sit far above any car loan, so lender
rules still decide every real case.

**Guarantee:** a request with no input errors is always `Evaluated` — no exception, no
negative amounts: every amount present is zero or more. `WellFormedRequestGuaranteeTests`
enforces it.

### Rule results — `LoanRuleResult`

| Field | Type | Notes |
|---|---|---|
| `IsValid` | bool | |
| `Code` | `LoanRuleCode` | what the rule checks — the same for every lender |
| `Parameters` | `IReadOnlyDictionary<LoanRuleParameter, decimal>?` | the limits (and for `CarAge` the actual combined years) behind a failure; `null` when valid |
| `ErrorMessage` | string? | developer diagnostic in English; **the UI does not show it** |

`LoanRuleCode`: `MinimumLoanAmount`, `MaximumLoanAmount`, `MinimumDownPayment`,
`MinimumLoanPeriod`, `MaximumLoanRatio`, `MaximumLoanPeriod`, `CarAge`.

The UI words each reason from `Code` plus `Parameters` (N2), so one message per code works
for every lender. A lender's own spec numbering (`A1`, `T3` in `docs/rules/lykill.md`) is
deliberately not on the boundary — it differs per lender; the engine tests trace codes back
to the spec instead.

| Code | Parameters on failure |
|---|---|
| `MinimumLoanAmount`, `MinimumDownPayment`, `MinimumLoanPeriod` | `Minimum` |
| `MaximumLoanAmount`, `MaximumLoanRatio` | `Maximum` (`MaximumLoanRatio`'s is a percent, e.g. `90`) |
| `MaximumLoanPeriod` | `MaximumLoanPeriodMonths` (general limit), or `LoanRatioThreshold` + `MaximumLoanPeriodMonths` (used-car band) |
| `CarAge` | `MaximumCombinedYears`, `LoanRatioThreshold`, `CombinedYears` (unrounded, e.g. `11.5833…`; the UI formats it) |

Each code reports one broken limit, so the UI never has to work out which of two limits
the user crossed. A loan that breaks both the ratio cap and the term fails both codes.
The parameter sets above are exact — the engine tests assert the full key set.

A declined loan still returns a result. The UI decides how to present a declined lender; it
does not filter the list on its own without a story saying so.

## Division of responsibility

- The engine owns every number: rates, fees, payments, eligibility.
- The UI owns presentation only: layout, labels, units, ordering, formatting, loading and
  error states.
- **The UI never does loan math.** If a screen needs a derived value, the contract gains a
  field; the UI does not calculate it.

## Changing this contract

A change here is a change to both sides. Neither builder edits it alone to make its own
work compile — raise it, agree the change, then both sides follow.
