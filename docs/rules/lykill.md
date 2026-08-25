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
| LTV / financing ratio | Loan amount as a percentage of the purchase price. |
| Term | Loan length. Given in years in the eligibility rules, in months in the fee schedule. |
| Green | Electric or hydrogen vehicles running on 100% renewable energy. Plug-in hybrids are **not** green. |

All amount and ratio checks run on the **pre-fee** loan amount.

---

## 2. Vehicle categories

| ID | Category |
|---|---|
| V1 | Petrol / diesel |
| V2 | Plug-in hybrid |
| V3 | Electric / hydrogen on 100% renewable energy |

V1 and V2 share the general rate table (§5). V3 uses the green rate table.
V2 and V3 each get their own fee discount (§6) — different discounts, not interchangeable.

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

### 4.2 Car age

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
| FR1 | 1–23 months | 1.80% |
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

F1 and F2 are different operations. F1 halves the computed fee; F2 subtracts
from the rate before it is applied. On a 72–84 month contract the effective
rates are 3.20% general, 2.20% plug-in hybrid, 1.60% green.

Both discounts are published as "up to" maximums. Compute the maximum and
present it to the user as the amount saved.

---

## 7. Calculation steps

1. Loan amount = purchase price − down payment.
2. Validate A1, A2, A3.
3. LTV = loan amount ÷ purchase price.
4. Validate T1/T2/T3 for the vehicle's new-or-used status.
5. If used, validate C1 or C2 for the applicable LTV band.
6. Look up the interest rate: `R-GRN` for V3, `R-GEN` for V1 and V2.
7. Look up the fee rate by term in months (FR1–FR6).
8. Apply F2 if V2 — subtract 1.00 percentage point from the fee rate.
9. Compute the fee on the loan amount.
10. Apply F1 if V3 — halve the fee.
11. Apply F4 — raise the fee to 18,000 kr. if it came out lower.
12. Add the fee on top of the loan (F5).

The fee never re-enters steps 2–5. Those checks are already done against the
pre-fee loan amount and are not revisited.
