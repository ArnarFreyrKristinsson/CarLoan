# CarLoan — User Stories: Single-Lender Calculator (Lykill)

One screen. Inputs on one side, results on the other. Every change recalculates
instantly. The result is either the full loan with everything in it, or a clear
statement that the loan is not available on these terms, and why.

Rule IDs (`A1`, `T3`, `F6`, …) refer to `docs/rules/lykill.md`, the source of truth for
all calculation and eligibility behaviour. These stories define what the user sees and
does, not how the numbers are computed.

UX rules (`docs/ux-rules.md`) apply to every story and are not repeated here.

Story IDs use the `US-` prefix so they never clash with the UX rule IDs (`S1`–`S8`).

## Prerequisite: spec change

Add to `lykill.md` §7 before implementing US-2:

> **P5** — Total interest: the monthly payment (rounded, as in §7) × the term in months,
> minus the financed amount.

## Out of scope

- Applying for a loan
- More than one lender, and comparison between lenders
- Saving or sharing a calculation

## Language and display

These apply to every story.

1. All text on screen is in Icelandic. Example texts in these stories are in English;
   on screen they appear in Icelandic.
2. Numbers use Icelandic format: `.` between thousands, `,` for decimals
   (`1.331.826 kr.`, `10,75%`).
3. Every amount in krónur is shown as a whole króna, rounded to the nearest one;
   halves round up.
4. The financing ratio is shown with one decimal, always rounded **up**, so it never
   appears on the wrong side of a rate or rule limit (80,03% shows as 80,1%; 79,96%
   shows as 80,0%).
5. The down payment percentage is 100% minus the financing ratio as displayed.
6. The interest rate and the origination fee rate are shown exactly as they apply
   (`10,75%`, `1,60%`).

## Reference example

Used in the acceptance criteria below. From `lykill.md` §7, plus a car age:

| Input | Value |
|---|---|
| Vehicle type | Electric / hydrogen (V3) |
| New or used | Used, 5 years old |
| Purchase price | 5.000.000 kr. |
| Down payment | 1.000.000 kr. |
| Term | 84 months |

| Output | Shown on screen |
|---|---|
| Loan amount | 4.000.000 kr. |
| Financing ratio | 80,0% |
| Down payment percentage | 20,0% |
| Interest rate | 10,75% |
| Origination fee rate | 1,60% |
| Origination fee | 64.000 kr. |
| Amount saved | 64.000 kr. |
| Financed amount | 4.064.000 kr. |
| Monthly payment | 69.053 kr. |
| Total interest | 1.736.413 kr. |

---

## US-1 — See my monthly payment

**As** someone buying a car,
**I want** to enter the price, my down payment and the loan length,
**so that** I can see what I would pay each month.

**Acceptance criteria**

1. The screen shows inputs for purchase price, down payment and term (in months).
2. WHEN the inputs give an allowed loan, THE SCREEN SHALL show the monthly payment as
   the most prominent result (P1–P4).
3. WHEN the user enters the reference example, THE SCREEN SHALL show a monthly
   payment of 69.053 kr.

*Starting state*

4. WHEN the screen first loads, it SHALL already show a complete, allowed loan with
   these values:

   | Input | Value |
   |---|---|
   | Vehicle type | Electric/hydrogen |
   | New or used | New |
   | Purchase price | 5.000.000 kr. |
   | Down payment | 1.000.000 kr. |
   | Term | 60 months |

5. WHEN the screen first loads, no rule message SHALL be shown.

## US-2 — See everything that makes up the payment

**As** someone buying a car,
**I want** to see every figure behind the monthly payment,
**so that** I understand what I'm paying for and am not surprised later.

**Acceptance criteria**

1. Alongside the monthly payment, THE SCREEN SHALL always show:
   - loan amount
   - financing ratio
   - interest rate (§5)
   - origination fee rate, after any F2 discount (§6)
   - origination fee amount (F1–F4)
   - financed amount (F5)
   - total interest (P5)
2. WHEN the vehicle type is plug-in hybrid or electric/hydrogen, THE SCREEN SHALL
   show the amount saved (F6).
3. WHEN the vehicle type is petrol/diesel, THE SCREEN SHALL NOT show an amount saved
   (it is always 0 for V1).
4. WHEN the user enters the reference example, THE SCREEN SHALL show every value in
   the reference output table.

## US-3 — Choose my vehicle type

**As** someone buying a car,
**I want** to say what kind of car it is,
**so that** I get the rate and fee that apply to it.

**Acceptance criteria**

1. THE SCREEN SHALL offer exactly three vehicle types: petrol/diesel (V1), plug-in
   hybrid (V2), electric/hydrogen (V3).
2. Exactly one vehicle type is selected at all times.
3. WHEN V3 is selected, THE SCREEN SHALL use the green rate table (R-GRN,
   `lykill.md` §5.2) and the 50% fee discount (F1, §6.1).
4. WHEN V2 is selected, THE SCREEN SHALL use the general rate table (R-GEN,
   `lykill.md` §5.1) and the 1.00 percentage point fee-rate discount (F2, §6.1), and
   not F1 (F3).
5. WHEN V1 is selected, THE SCREEN SHALL use the general rate table (R-GEN,
   `lykill.md` §5.1) with no fee discount.
6. WHEN the user changes vehicle type, every result SHALL update immediately.

## US-4 — Tell whether the car is new or used

**As** someone buying a used car,
**I want** to give the car's age,
**so that** the rules for used cars are applied to my loan.

**Acceptance criteria**

1. THE SCREEN SHALL offer a choice between new and used.
2. WHEN used is selected, THE SCREEN SHALL show a car age input (in years).
3. WHEN new is selected, THE SCREEN SHALL hide the car age input and SHALL NOT apply
   C1/C2.
4. WHEN the user switches from used to new and back, THE SCREEN SHALL restore the car
   age they had entered.
5. WHEN used is selected, eligibility SHALL follow T2/T3 and C1/C2; WHEN new is
   selected, T1.

## US-5 — Know when I can't get the loan, and why

**As** someone buying a car,
**I want** to be told clearly when the loan isn't available on my terms,
**so that** I know what to change.

**Acceptance criteria**

*Messages*

1. WHEN any of A1–A3, T1–T4, C1–C2 fails, THE SCREEN SHALL show a message for that
   rule.
2. WHEN several rules fail, THE SCREEN SHALL show a message for each of them.
3. Each message SHALL state what is wrong, the limit that applies, and what the user
   can change to fix it. Example (T3):
   > "On a used car with over 80% financing, the maximum term is 72 months. Shorten
   > the term or raise the down payment."
4. Messages SHALL NOT contain rule IDs or other codes.
5. Messages SHALL refer to the financing ratio by the same name used where it is
   displayed.
6. WHEN the inputs change so a rule passes again, its message SHALL disappear.

*Results when the loan is not available*

7. WHEN a rule fails but the payment can still be calculated, THE SCREEN SHALL show
   the monthly payment and total interest in a muted state, labelled "Not available
   on these terms".
8. WHEN the payment cannot be calculated, THE SCREEN SHALL show no number for the
   monthly payment and total interest. In their place it SHALL state that the payment
   cannot be calculated, and why:

   | Situation | Why it can't be calculated | Example text |
   |---|---|---|
   | Financing ratio above 90% | No interest rate exists above 90% (§5) | "The monthly payment can't be calculated: there is no interest rate for financing above 90%." |
   | Term under 6 or over 84 months | No origination fee exists outside 6–84 months (§6) | "The monthly payment can't be calculated: loans are only offered for 6 to 84 months." |
   | Loan amount is 0 | There is no loan | "The monthly payment can't be calculated: the down payment covers the whole price, so there is no loan." |
9. WHEN all rules pass, the results SHALL return to their normal state.

## US-6 — Adjust the numbers and see the effect instantly

**As** someone exploring what I can afford,
**I want** to move sliders and see the results change as I go,
**so that** I can find terms that work for me.

**Acceptance criteria**

*Feedback*

1. Purchase price, down payment and term each have both a slider and a typed input,
   kept in sync.
2. WHEN the user moves a slider or changes a choice, every result and message SHALL
   update immediately, with no button to press.
3. WHEN the user types into a field, results and messages SHALL update after a short
   pause in typing or when the user leaves the field, not on every keystroke.
4. The down payment SHALL be shown both in kr. and as a percentage of the purchase
   price.

*Fixed limits (the same for every loan): slider ends*

5. The purchase price slider SHALL run from 900.000 kr. to 20.000.000 kr., in steps
   of 50.000 kr. (900.000 kr. is the lowest price any loan is possible for: A1 + A2.)
6. The down payment slider SHALL run from 150.000 kr. (A2) to the purchase price.
7. The term slider SHALL run from 6 to 84 months.

*Moving limits (depend on other inputs): marker and message, never a cap*

8. WHEN the maximum allowed term is below 84 months (T3, C1, C2), THE SCREEN SHALL
   show a marker on the term slider at that maximum.
9. The user SHALL be able to move the term slider past the marker; the relevant
   US-5 message then appears.
10. Changing other inputs SHALL NOT change the term the user has set.

    Example: used car, 8 years old, financing 85% → C1 allows 4 years, so the marker
    sits at 48 months. Lowering the financing to 80% moves the marker to 84 months
    (C2 allows 12 years, T2 allows 84 months). The term the user set stays where it
    was.

*Typed values*

11. Typed fields SHALL accept digits only.
12. WHEN the user types a value outside a slider's range, THE SCREEN SHALL keep the
    typed value, place the slider at its nearest end, and show any rule message that
    applies. Examples: a term of 3 months shows the T4 message; a down payment of
    100.000 kr. shows the A2 message; a purchase price of 35.000.000 kr. is accepted
    as is.
13. THE SCREEN SHALL NOT change a value while the user is still typing it.

*Down payment above the price*

14. WHEN the user lowers the purchase price below the current down payment, THE SCREEN
    SHALL reduce the down payment to the new purchase price and briefly highlight the
    down payment field to show it changed.
15. WHEN the user types a down payment above the purchase price, THE SCREEN SHALL,
    when the user leaves the field, reduce it to the purchase price and briefly
    highlight it.
