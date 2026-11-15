# Lykill — Car Loan Validation Rules

Reference spec for validating and pricing a car loan.

Every rule has a stable ID (`A1`, `T2`, `F4`, …).

---

## 1. Terminology

| Term | Meaning |
|---|---|
| Purchase price | The price of the car. |
| Down payment | The buyer's own contribution. A single field. |
| Loan amount | Purchase price minus down payment. **Excludes fees.** |
| Financed amount | Loan amount plus the origination fee (F5). Used only for the monthly payment (§7). |
| LTV / financing ratio | Loan amount as a percentage of the purchase price. |
| Term | Loan length. Given in years in the eligibility rules, in months in the fee schedule and the monthly payment. |
| Green | Electric or hydrogen vehicles, whatever energy they run on (V3). Plug-in hybrids are **not** green. |

All amount and ratio checks run on the **pre-fee** loan amount.

---

## 2. Vehicle categories

| ID | Category |
|---|---|
| V1 | Petrol / diesel |
| V2 | Plug-in hybrid |
| V3 | Electric / hydrogen |

V1 and V2 share the general rate table (§5). V3 uses the green rate table.
V2 and V3 each get their own fee discount (§6) — different discounts, not interchangeable.

V3 is every electric or hydrogen vehicle. The category depends on the drivetrain alone:
the published wording "on 100% renewable energy" is not a further condition, so an
electric or hydrogen vehicle that does not run on 100% renewable energy is still V3, and
there is no separate category for it.

---

## 3. Amount rules

| ID | Rule |
|---|---|
| A1 | Loan amount ≥ 750,000 kr. |
| A2 | Down payment ≥ 150,000 kr. |
| A3 | Loan amount ≤ 30,000,000 kr. |

---

## 4. LTV, term, and age rules

### 4.1 LTV and term

| ID | Condition | Max LTV | Max term |
|---|---|---|---|
| T1 | New car | 90% | 7 years (84 months) |
| T2 | Used car | 80% | 7 years (84 months) |
| T3 | Used car | >80% to 90% | 6 years (72 months) |

T2 and T3 are two bands of the same rule: a used car at or below 80% LTV may run
7 years; above 80% and up to 90% it is capped at 6 years.

### 4.2 Minimum term

| ID | Rule |
|---|---|
| T4 | Term ≥ 6 months (0.5 years). Applies to every car, new or used. |

### 4.3 Car age

Applies to **used cars only**. New cars are exempt.

| ID | Condition | Rule |
|---|---|---|
| C1 | Used car, LTV >80% to 90% | `carAgeYears + termYears ≤ 12` |
| C2 | Used car, LTV 0 to 80% | `carAgeYears + termYears ≤ 20` |

---

## 5. Interest rate

Keyed on financing ratio (LTV). Not affected by the down payment amount, the
term, or anything else.

### 5.1 General — `R-GEN` (V1 petrol/diesel, V2 plug-in hybrid)

| ID | Financing ratio | Rate |
|---|---|---|
| R-GEN-1 | ≤ 50% | 10.35% |
| R-GEN-2 | > 50% to 70% | 11.20% |
| R-GEN-3 | > 70% to 80% | 11.45% |
| R-GEN-4 | > 80% to 90% | 12.20% |

### 5.2 Green — `R-GRN` (V3 electric/hydrogen)

| ID | Financing ratio | Rate |
|---|---|---|
| R-GRN-1 | ≤ 50% | 9.65% |
| R-GRN-2 | > 50% to 70% | 10.50% |
| R-GRN-3 | > 70% to 80% | 10.75% |
| R-GRN-4 | > 80% to 90% | 11.50% |

`R-GRN` sits exactly 0.70 percentage points below `R-GEN` in every band. It can
be modelled as a second table or as a modifier on the first — the published
schedule gives both forms.

Band boundaries are inclusive at the top, exclusive at the bottom: 70.0% falls
in band 2, 70.01% in band 3.

---

## 6. Origination fee

Charged on the loan amount, keyed on contract length in months.


| ID | Contract length | Fee rate |
|---|---|---|
| FR1 | 6–23 months | 1.80% |
| FR2 | 24–35 months | 2.00% |
| FR3 | 36–47 months | 2.25% |
| FR4 | 48–59 months | 2.50% |
| FR5 | 60–71 months | 3.00% |
| FR6 | 72–84 months | 3.20% |

### 6.1 Fee rules

| ID | Rule |
|---|---|
| F1 | Green (V3): up to 50% off the fee **amount** |
| F2 | Plug-in hybrid (V2): up to 1.00 percentage point off the fee **rate** |
| F3 | Plug-in hybrid does not qualify for F1 |
| F4 | Minimum fee of 18,000 kr., applied **after** the F1/F2 discount |
| F5 | The resulting fee is added on top of the loan |
| F6 | Amount saved: the fee without the F1/F2 discount minus the fee charged, **both after F4** |

F1 and F2 are different operations. F1 halves the computed fee; F2 subtracts
from the rate before it is applied. On a 72–84 month contract the effective
rates are 3.20% general, 2.20% plug-in hybrid, 1.60% green.

Both discounts are published as "up to" maximums. Apply the maximum, and present
what it saves the buyer as the amount saved (F6).

The amount saved is what the buyer pays less, not the size of the discount: F4 can
take back part or all of a discount, and F6 counts only what is left. It is never
negative. It is 0 for V1, and whenever the fee without the discount is 18,000 kr. or
less. Green car, 1,000,000 kr. loan, 84 months: without F1 the fee is 32,000 kr.;
F1 halves it to 16,000 kr.; F4 raises that to 18,000 kr. The amount saved is
32,000 − 18,000 = 14,000 kr., not the 16,000 kr. F1 took off.

---

## 7. Monthly payment

| ID | Rule |
|---|---|
| P1 | Equal monthly payments over the whole term (an annuity): the same amount every month, covering interest and principal together. |
| P2 | The principal is the financed amount: loan amount plus the fee (F5). |
| P3 | The §5 rate is a nominal annual rate. The monthly rate is one twelfth of it. |
| P4 | The number of payments is the term in months. |
| P5 | Total interest: the monthly payment (rounded, as in §7) × the term in months, minus the financed amount. |

`payment = financed amount × r × (1 + r)^n ÷ ((1 + r)^n − 1)`, where `r` is the
monthly rate as a fraction (10.75% → `0.1075 ÷ 12`) and `n` is the term in months.
The payment is rounded to 2 decimals.

Example: a used electric car, purchase price 5,000,000 kr., down payment 1,000,000 kr.,
84 months.

| Step | Value |
|---|---|
| Loan amount | 4,000,000 kr., 80% LTV |
| Rate | 10.75% (R-GRN-3) |
| Fee | 4,000,000 × 3.20% = 128,000 kr., halved by F1 to 64,000 kr. — above the F4 minimum |
| Financed amount | 4,064,000 kr. |
| Monthly payment | `r = 0.1075 ÷ 12`, `n = 84` → 69,052.53 kr. |

---

## 8. Calculation steps

1. Loan amount = purchase price − down payment.
2. Validate A1, A2, A3.
3. LTV = loan amount ÷ purchase price.
4. Validate T1/T2/T3 for the vehicle's new-or-used status, and T4 for the minimum term.
5. If used, validate C1 or C2 for the applicable LTV band.
6. Look up the interest rate: `R-GRN` for V3, `R-GEN` for V1 and V2.
7. Look up the fee rate by term in months (FR1–FR6).
8. Apply F2 if V2 — subtract 1.00 percentage point from the fee rate.
9. Compute the fee on the loan amount.
10. Apply F1 if V3 — halve the fee.
11. Apply F4 — raise the fee to 18,000 kr. if it came out lower.
12. Add the fee on top of the loan (F5). This is the financed amount.
13. Amount saved (F6): the fee at the step-7 rate with no discount, raised to 18,000 kr.
    if lower, minus the fee from step 11.
14. Monthly payment (P1–P4): the financed amount from step 12, at the step-6 rate, over
    the term in months.

The fee never re-enters steps 2–5. Those checks are already done against the
pre-fee loan amount and are not revisited.
