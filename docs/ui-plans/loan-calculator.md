# Loan calculator

**Story:** US-1 to US-6 in `docs/spec/user-stories.md`
**Status:** planned

## Story review settlement

`ux-reviewer` reviewed the stories before anything was built (2026-11-02) and reported 15
findings. The story file defines correct, so it is not rewritten. Each finding is settled
below, and the screen and the engine are built to the settlement. Three of them were decided by
a human: the contract change (1, 3), the down payment after a price cut (6) and the
first car age (8).

| # | Finding | Sev | Settled as |
|---|---|---|---|
| 1 | The stories need figures the contract does not carry | 4 | The contract gains them: loan amount, financing ratio rounded up to one decimal, down payment percentage (100 − that ratio, so Language 5 is the engine's), financed amount, total interest (P5), whether the loan is accepted, and the reasons a price is missing. Human decision: `MonthlyPayment`, `InterestRate` and `OriginationFee` become nullable, null exactly when the lender's schedule has no price for the terms. |
| 2 | The term marker needs eligibility logic, edge cases undefined | 4 | The engine reports the longest term the term and car-age rules allow. The marker shows when that is from 6 up to 83 months. At 84 it is not shown (AC10's "moves to 84" means the marker leaves the slider). Below 6, or when there is none, there is no marker; the rule message says what to change. |
| 3 | "Always show" against "can't be calculated" | 4 | Every US-2 row is always on screen. A figure the lender has no price for shows "—", never a number: above 90% financing the interest rate. Outside 6–84 months: the fee rate, fee, amount saved and financed amount. With no loan (down payment covers the price): every loan figure. Whenever a rule fails, all figures are muted with the payment, not only the payment. |
| 4 | No story covers `InvalidRequest` | 3 | An empty field: the results ask for that field and show no rule message. A price or term of 0, or a value above the contract's `Maximum…` constants: a plain message beside that field, and the payment area says why it can't be calculated. The fields take no more digits than those constants need. A down payment above the price while the user is still in the field: the "no loan" statement, until the user leaves the field (AC15). |
| 5 | "Digits only" against the Icelandic format and pasting | 3 | Typing inserts digits only. The dots already shown stay as separators while editing and are ignored when the value is read. The field is reformatted when the user leaves it. A paste keeps the digits before a decimal comma and drops "kr.", spaces and dots: "5.000.000 kr." gives 5.000.000, and "4.500.000,50" gives 4.500.000, never 450.000.050. |
| 6 | AC14 overwrites the down payment and never restores it | 3 | Human decision: the down payment the user set comes back when the price rises again (S6). A typed price cuts the down payment only when the user leaves the price field, never during a typing pause. |
| 7 | Which fee rate is shown | 3 | The rate as it applies, as in the reference table and Language 6: 1,60% for V3, 2,20% for V2 on 72–84 months. When F4 raised the fee to the minimum, the fee shows a "minimum fee" note, from an engine flag. |
| 8 | No car age on the first switch to used | 3 | Human decision: empty, and the results ask for the car's age until it is entered. |
| 9 | No boundary scenarios | 2 | The engine tests cover every limit in `lykill.md` (`LykillBoundaryTests` plus the new pricing tests). The self-check walks a sample of them on the screen. |
| 10 | Rule message and "can't be calculated" can repeat | 2 | Both show: the rule message says what to change, and the one-line statement says why there is no number. Two reasons at once are both named in the statement. One message per failing rule code. |
| 11 | Engine-side criteria, unmeasured timings | 2 | The rate and fee table choices are checked through engine tests and the figures they put on screen. Typing pause: 600 ms. Highlight: 2 seconds. |
| 12 | How car age is counted is not stated | 2 | For human decision (below). The field takes whole years, the only unit the contract has. |
| 13 | Combined years can round onto the limit | 2 | Car-age messages state the limit and what to change, not the computed total. |
| 14 | Slider definitions incomplete | 2 | Down payment slider steps of 10.000 kr., term slider steps of 1 month. A typed value off the step keeps its exact value, and the slider sits at the nearest step. With a price under 150.000 kr. the down payment slider is disabled at its minimum. |
| 15 | Criteria not in Given/When/Then | 2 | For human decision (below). Not rewritten here, because the story file defines correct. |

Needs user test, from the review: whether a muted payment reads as "not an offer". Whether a
typed value with its slider parked at the end looks like a mismatch. Whether "amount saved"
on a small V2 or V3 loan looks like a fault. Whether hand-checkers distrust total interest (P5
uses the payment before it is rounded to whole krónur). Whether the brief highlight is noticed.

## Rule plan

To be written before the screen is built.

## Acceptance criteria

To be filled in at the self-check.

## Findings applied

None yet: `docs/ux-findings.md` has no entries.

## Self-check

Not run yet.

## Review rounds

None yet.

## Engine review

The engine stage's `engine-reviewer` loop.

| Round | Finding | Sev | Action |
|---|---|---|---|
| 1 | Lykill's general rate table untested at its 90% edge | 3 | Fixed: `LykillPricingBoundaryTests`, every §5 and §6 band edge through the service |
| 1 | Each price and its gap come from two calls; the default rates and the fee band fallback still exist in the Domain | 2 | Declined at first: the fix rewrites existing Domain tests, beyond the agreed scope. Fixed after a human decision: each lookup returns its price or null, the gap is derived from the same lookup, and the default rates are gone |
| 1 | The guarantee test's priced extremes were never checked as priced | 2 | Fixed |
| 1 | The term limit is stated in `Evaluate` and in `MaximumLoanPeriodInMonths` with no test holding them together | 2 | Fixed: `LykillLongestTermTests`, plus Domain agreement theories in round 2 |
| 1 | `OriginationFeeSettings.MinimumContractMonths` defaults to 1, so a lender set up without it prices terms its schedule lacks | 2 | Declined at first for the same reason; fixed after the same human decision: the setting is required |
| 2 | A used-car limit longer than the general one was reported as the longest term | 2 | Fixed |
| 3 | An empty rate tier list was accepted and reported a 0–0% gap | 2 | Fixed after the last round: the constructor rejects it. Not re-reviewed |
| 3 | `MinimumContractMonths` was checked against the longest fee tier, so it could make the lower bands unreachable | 2 | Fixed after the last round: it must fall inside the shortest tier. Not re-reviewed |

Result after three rounds: 2 findings remained (both severity 2), and both were fixed after
the last round without another review.

## For human decision


- **Story 12, car age counting.** `lykill.md` §4.3 does not say what the car's age is counted
  from (first registration or model year) or how part years round. That decides C1/C2 near a
  limit. It belongs in `lykill.md`, then in the field's help text.
- **Story 15, criteria form.** The stories use "WHEN … SHALL" without a starting state, while
  `definition-of-done.md` asks for Given/When/Then. The settlement above stands in for the
  missing scenarios until the story file is updated.
