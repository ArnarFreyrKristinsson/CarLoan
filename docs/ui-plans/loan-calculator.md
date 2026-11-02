# Loan calculator

**Story:** US-1 to US-6 in `docs/spec/user-stories.md`
**Status:** built; two contract gaps open for human decision

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

Written before the screen was built. The design aim from the request: the user should feel
trust, clarity, calm, control and that they are taken seriously. Every rule below is read
through that aim.

| ID | How this screen satisfies it, or why it doesn't apply |
|---|---|
| N1 / S3 | Every change recalculates at once: sliders and choices immediately, typing after a 600 ms pause or on leaving the field. The payment area always says which of three states it is in: available ("Stenst lánareglur Lykils"), not on these terms (muted, labelled), or cannot be calculated (with the reason). Feedback is modest for frequent actions (numbers change in place). It is substantial for the rare system-made change: the down payment field is highlighted for 2 seconds and a note says why. On a narrow screen a compact payment bar stays at the bottom while the inputs scroll. |
| N2 | Icelandic throughout, in the lender's vocabulary (kaupverð, útborgun, lánsfjárhæð, fjármögnunarhlutfall, lántökugjald, vextir), kr. and %, terms in months. Icelandic number format, whole krónur. Inputs are ordered the way a buyer thinks: the car (type, new or used, age, price), then the financing (down payment, term). No rule IDs, field names or codes anywhere. |
| N3 / S6 / S7 | Nothing is committed and nothing is destructive. Every input is changed back the way it was changed. Switching new → used → new restores the car age. A down payment cut by a lower price comes back when the price rises (human decision). A typed value outside a slider's range is kept, and the term marker never caps the slider. The only change the screen makes on its own is that down payment cut, and the screen says so. Undo/redo: not applicable, because there is no committed action to undo. |
| N4 / S1 | One name per figure, used in the figures list and in every message ("fjármögnunarhlutfall", US-5 AC5). The three slider fields are one component, so they look and behave the same. The two choices are one component. Same number format everywhere. Native radio groups and range inputs, so keyboard behaviour follows the platform. |
| N5 / S5 | Fields accept digits only, and only as many as the engine's input bounds need. Sliders are bounded. The down payment cannot stay above the price. The term marker shows the longest allowed term before the user crosses it. Out-of-range values are kept, never corrected silently. |
| N6 / S8 | All inputs and all figures are on one screen at once. Slider ends are labelled, the down payment shows its percentage beside it, and the marker labels the longest term. Messages repeat the limit, so the user never has to remember it from elsewhere. |
| N7 / S2 | Sliders for exploring, typed fields for an exact number (the accelerator), arrow and Page keys on sliders. Universal usability: layout from 320 px up, every control reachable and operable by keyboard with a visible focus ring, labels tied to inputs, results in a polite live region, touch targets at least 44 px, AA contrast, reduced motion respected. Languages: Icelandic only, as the stories require. |
| N8 | One emphasised number (the payment). The figures are a quiet list. Messages appear only when a rule fails. No decoration, no marketing copy. One short footnote says what the numbers are based on and that they are not a loan offer. |
| N9 | Each message says what is wrong, the limit, and what to change, in plain Icelandic (US-5 AC3). "Can't be calculated" always says why. |
| N10 | No separate help: labels and the hints on the two fields that need one (car age in whole years, down payment as a share of the price) carry it. The footnote says where the numbers come from. |
| S4 | Each change ends with a clear result state, so the user knows when the loan works: "Stenst lánareglur Lykils", or the reasons it does not. |

### Layout and tone

Inputs on the left in two groups: the car (type, new or used, age, price), then the
financing (down payment, term). Results on the right, sticky while the inputs scroll. The
monthly payment is the one large number, with total interest beneath it and a plain status
line. Below that come the messages, only when a rule fails, and then the figures as a quiet
list.

The palette is cool neutrals with one deep teal accent for controls and the selected state.
A loan that is not on these terms gets amber, not red: the screen informs rather than scolds.
Red is kept for a value the engine cannot evaluate at all. The type is the system font with
tabular figures, so numbers do not jitter as they change. There is no imagery, no marketing
copy and no exclamation marks. One footnote says what the numbers rest on and that they are
not a loan offer. That honesty is what earns trust.

The only decisions the UI makes are which engine fact to show and how to word it: a figure
or "—", one of three payment states from the result's nulls and `IsAccepted`, one message per
failed rule code. It computes no figure.

### Change: colours and a dark mode (2026-11-02)

Requested: colours that carry trust, clarity, calm, control and being taken seriously, and a
dark mode. Rules this change touches:

| ID | How the change satisfies it |
|---|---|
| N1 / S3 | The theme switches at once when chosen, and the toggle shows its state (`aria-pressed`, filled when dark is on). |
| N3 / S6 / S7 | The visitor decides. The page follows the system setting until they choose; the choice is remembered in their browser, and choosing the theme the system gives forgets it, so the same toggle is the way back to following the system. A theme that differs from the system stays until the visitor changes it; once they are back on the system's theme, the page follows the system again. |
| N4 / S1 | One set of colour tokens with the same meaning in both themes: teal for controls and focus, green for "stenst", amber for "not on these terms" and limits, red only for a value the engine cannot evaluate. |
| N8 | Dark is deep slate, not black, and the accent is lifted only enough to keep its contrast. Nothing new competes with the payment. |
| S2 / N7 | Every text colour is WCAG AA (4.5:1) on every background it sits on, and control edges and focus are 3:1 or more, in both themes; computed, not eyeballed. The light theme's text-field border was about 1.6:1 and is now 3:1. Native controls follow the theme (`color-scheme`). |
| N2, N5, N6, N9, N10, S4, S5, S8 | Not changed by colour alone. |

## Acceptance criteria

Checked against the running screen (2026-11-02) by driving it through each state and reading
what it showed. "Engine" means the behaviour is proven by the engine tests, and the screen only
shows the result.

| Criterion | How the built screen satisfies it |
|---|---|
| US-1 AC1 | Purchase price, down payment and term, each a typed field with a slider, with the term in months. |
| US-1 AC2 | The payment is the one large number at the top of the results, with only total interest and the status line beside it. |
| US-1 AC3 | Reference example (used V3, 5 years, 5.000.000, 1.000.000, 84 months) shows 69.053 kr. |
| US-1 AC4 | The screen opens on V3, new, 5.000.000 kr., 1.000.000 kr., 60 months, already calculated (87.769 kr.). The page is rendered with it, so there is no empty first frame. |
| US-1 AC5 | Starting state shows no rule message. Its loan passes every rule. |
| US-2 AC1 | Loan amount, financing ratio, interest rate, fee rate, fee amount and financed amount are always listed. Total interest sits under the payment. A figure the engine has no number for shows "—". |
| US-2 AC2 | "Sparnaður á lántökugjaldi" is shown for plug-in hybrid and electric/hydrogen. |
| US-2 AC3 | Not shown for petrol/diesel. |
| US-2 AC4 | Reference example shows 4.000.000 kr., 80,0%, 20,0% (beside the down payment), 10,75%, 1,60%, 64.000 kr., 64.000 kr., 4.064.000 kr., 69.053 kr. and 1.736.413 kr. |
| US-3 AC1–2 | One radio group with exactly the three types, and one always selected. |
| US-3 AC3–5 | Engine: the rate table and fee discount per type (`LykillPricingTests`, `LykillPricingBoundaryTests`). The screen shows 10,75% / 1,60% for V3 and 11,45% / 2,20% for V2 on the reference loan. |
| US-3 AC6 | Changing the type re-evaluates at once. |
| US-4 AC1–2 | New/used choice. Used shows "Aldur bílsins" in whole years. |
| US-4 AC3 | New hides the field and sends age 0, which C1/C2 ignore for a new car (engine). |
| US-4 AC4 | Used → new → used restores the age entered (checked with 7). |
| US-4 AC5 | Engine: T1–T3, C1–C2 (`LykillBoundaryTests`, `LykillLongestTermTests`). |
| US-5 AC1–2 | One message per failed rule code. A used car aged 8 at 85% over 84 months shows both the T3 and the C1 message. |
| US-5 AC3 | Each message gives the problem, the limit and the change to make. The T3 message is the story's example, in Icelandic. |
| US-5 AC4 | No rule IDs or codes appear anywhere. |
| US-5 AC5 | "Fjármögnunarhlutfall" in the figures and in every message. |
| US-5 AC6, AC9 | Messages and states are rebuilt from each evaluation, so a passing rule's message disappears and the normal state returns. |
| US-5 AC7 | The payment and total interest turn grey with the "Ekki í boði á þessum kjörum" badge, and the figures are muted with them. |
| US-5 AC8 | No number. "Ekki er hægt að reikna mánaðarlega greiðslu" with the reasons: no rate above 90%, loans only for 6 to 84 months, no loan when the down payment covers the price. Both reasons show when both apply. |
| US-6 AC1 | Price, down payment and term each have a slider and a typed field, kept in sync. |
| US-6 AC2 | Sliders and choices re-evaluate on every movement, with no button. |
| US-6 AC3 | Typing re-evaluates after a 600 ms pause or on leaving the field. |
| US-6 AC4 | The down payment shows in kr. and as "x% af kaupverði" (engine's figure). |
| US-6 AC5–7 | Price 900.000–20.000.000 in 50.000 steps. Down payment 150.000 up to the price. Term 6–84. |
| US-6 AC8–9 | Marker "Hámark 48 mán." at the longest allowed term when it is below 84. The slider moves past it freely; the fill turns amber past the marker and the rule message appears. |
| US-6 AC10 | Nothing but the user changes the term. |
| US-6 AC11 | Only digits can be typed. A paste keeps the digits before a decimal comma, and the fields take no more digits than the engine's bounds need. |
| US-6 AC12 | Typed 3 months: kept, slider at 6, T4 message. Typed 100.000 kr. down: kept, A2 message. Typed 35.000.000 kr.: kept, slider at its 20.000.000 end. |
| US-6 AC13 | A field is never rewritten while it has focus. A typed price of "4" left the down payment at 1.000.000 during the pause. |
| US-6 AC14 | Price slider to 900.000: the down payment drops to 900.000 and is highlighted, with a note. Price back to 5.000.000: it returns to 1.000.000, also with a note (human decision). |
| US-6 AC15 | A typed 6.000.000 kr. down payment on 5.000.000 shows "no loan" while in the field, and comes down to 5.000.000, highlighted, on leaving it. |

## Findings applied

None yet: `docs/ux-findings.md` has no entries.

## Self-check

2026-11-02, against the screen as built: every rule ID, then every criterion above, on desktop
(1280 px) and phone (375 px) widths. Fixed before review:

| Rule | What was wrong | What changed |
|---|---|---|
| N8 / S1 | Every slider had a sliver of amber at its right end, even with no marker. | The shaded "past the limit" track only appears on a slider with a marker. |
| N1 / N6 | Past the marker, the track looked the same as an allowed term. | The fill turns amber from the marker on. |
| N9 | An empty car age read as an error ("Ekki er hægt að reikna…"). | A missing input has its own calm state that just asks for the value. |
| S2 | At 375 px "Tengiltvinnbíll" filled its segment edge to edge, and narrower phones would overflow it. | Below 420 px the three vehicle types stack. |
| N8 | "Lánsfjárhæð með lántökugjaldi" pushed "kr." onto its own line on a phone. | Figure values never wrap. |
| N2 | Messages ending in an amount read "kr..". | The amount's own full stop ends the sentence. |
| N5 / S7 | Typing before the page went live was overwritten when it did, which once turned a typed 100.000 into 100. | Fields, sliders and choices take no input until the page is live, a fraction of a second after it appears. A value can no longer change silently. |
| — | An Icelandic singular month rule was logic in the UI with no test. | Removed: every term Lykill's rules name takes the plural. |

### Self-check: colours and dark mode (2026-11-02)

Checked both themes on the running screen at 1280 and 375 px (start, declined, no payment), and
computed every colour pair. The theme follows the system setting with no saved choice, the
toggle overrides it, and the choice survives a reload. Every text pair is 4.5:1 or more in both
themes: the lowest is quiet text on the page background, 5.07 light and 7.05 dark. Control
edges and focus are 3:1 or more.

| Rule | What was wrong | What changed |
|---|---|---|
| S2 / N7 | In the light theme, a text field's border was about 1.6:1 against white, below the 3:1 a control edge needs. | Border darkened to 3.14:1. |
| S1 / N4 | Shadows and the reconnect overlay used fixed colours that would not follow a theme. | They use theme tokens. |

## Review rounds

### Round 1 — 2026-11-02
| Finding | Rule | Severity | Action |
|---|---|---|---|
| The car-age message cannot say which band failed: no 80% threshold, no "raise the down payment", and the 0-term wording suggested another ratio would help in the C2 band | AC US-5 AC3, N9 | 3 | Partly fixed, and the round-1 wording overstated the cap (round 2, finding 1). The rest is a contract gap, for human decision (below) |
| "x% af kaupverði" disappears when the engine returns no `LoanFigures` (an empty field, down payment ≥ price, input out of bounds) | AC US-6 AC4 | 2 | Contract gap, for human decision (below). The UI may not compute it |
| Tabbing through the down payment after a price cut made the cut value the remembered one | S6/N3 | 2 | Fixed: leaving a field counts as a choice only if the user typed in it |
| The restore note could be untrue, and stayed up after the user set the down payment | N1/S3 | 2 | Fixed: three wordings (cut, back to your choice, raised with the price), and the note and highlight clear when the user acts |
| On a phone, messages sat below every input | S8/N6 | 2 | Fixed: on narrow screens each message is repeated beside the input that fixes it (down payment or term) |
| Screen readers were not told a message appeared | S2/N7 | 2 | Fixed: a polite live region announces added messages only |
| A refused comma or paste was silent; other number formats were misread | S5/N5 | 2 | Fixed: a note says why; a paste is taken only if it reads as an Icelandic amount, otherwise refused |
| Slider touch area 32 px | S2/N7 | 2 | Fixed: the input is 44 px tall, thumb 24 px |
| Total interest lost its row when there is no payment | AC US-2 AC1 | 1 | Fixed: the row stays, with "—" |
| An over-long term quoted the engine's 1.200-month bound | N9, N4/S1 | 1 | Fixed: "Lánstíminn er of langur. Styttu lánstímann." (and the same for the price bound) |

### Round 2 — 2026-11-02
| Finding | Rule | Severity | Action |
|---|---|---|---|
| The round-1 car-age wording stated the over-80% cap as the rule for every used car, and told a 12–19-year-old car at 85% that no term was offered, though at 80% one is | AC US-5 AC3, N9 | 3 | Wording fixed to what holds in both bands: "Miðað við núverandi forsendur mega aldur bílsins og lánstíminn samanlagt ekki vera meira en 12 ár…", and with no term left, "…enginn lánstími er í boði miðað við þessar forsendur". It no longer claims a universal cap or sends anyone to a change that cannot help. It still cannot offer the down payment as the fix in the C1 band, which stays open as a contract gap |
| On a phone, a message appearing beside a slider moved the slider mid-drag | S7/N3 | 2 | Fixed: the repeated messages come after the slider and its end labels |
| The "no loan" reason was not repeated beside the down payment on a phone | S8/N6 | 2 | Fixed: it is now |
| A typed dot got the comma note | S5/N9 | 2 | Fixed: a dot says the dots are added automatically, a comma says whole numbers only, anything else says digits only |
| Windows high-contrast mode would lose the selected option, the slider fill and the marker | S2/N7 | 2 | Fixed: in forced colours the selected option gets a system-colour outline, the sliders render natively and the marker uses a system-colour border |

### Round 3 — 2026-11-02
| Finding | Rule | Severity | Action |
|---|---|---|---|
| Keyboard focus on sliders and choices was only a 35%-alpha halo (about 1.8:1), and vanished in Windows high contrast | S2/N7 | 3 | Fixed after the last round: a solid accent outline on every focusable control, and in forced colours selection is a border and focus an outline, so both show. Not re-reviewed |
| Content above the down-payment slider still changed height mid-drag: the percentage line vanishing at the price end, and the change notice appearing or expiring | S7/N3 | 2 | Fixed after the last round: one fixed-height status line above each slider shows a refusal, else a notice, else the hint, and input errors moved below the slider. Checked on screen: the slider no longer moves. Not re-reviewed |
| The "dots are added" note was untrue in the car-age field | S5/N9 | 2 | Fixed after the last round: a short field calls a dot a decimal point and says whole numbers only, and the note stays up while the user keeps typing. Not re-reviewed |

**Result after three rounds:** 3 findings remained, the most severe a 3. All three were fixed
after the last round without another review. Two contract gaps stay open for human decision
(below).

### Colours and dark mode, round 1 — 2026-11-02
| Finding | Rule | Severity | Action |
|---|---|---|---|
| After Blazor's enhanced navigation the server HTML could put back an unpressed toggle and no theme | N1/S3, S7/N3 | 2 | Fixed: the theme is applied again after `enhancedload`. Checked: dark survives the error page and its link back |
| Every click saved a choice, so there was no way back to following the system | N3/S7 | 2 | Fixed: choosing the theme the system gives forgets the choice. Checked: after clicking back to light, the page follows the system to dark |
| In Windows high contrast the toggle did nothing and looked broken | S2/N7, N1/S3 | 2 | Fixed: hidden in forced colours |

### Colours and dark mode, round 2 — 2026-11-02
| Finding | Rule | Severity | Action |
|---|---|---|---|
| The plan said nothing switches on its own once chosen, but choosing the system's theme means following the system again | S7/N3 (plan vs screen) | 1 | Fixed: the plan now describes what the screen does. The behaviour is the round-1 fix and stays |

### Colours and dark mode, round 3 — 2026-11-02
No findings.

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

- **UX review round 1, contract gaps.** Two fixes need the API contract to carry more, which
  the UI skill says to raise rather than work around:
  1. A `CarAge` failure should say which band it is in (above the used-car threshold or not), so
     the message can name the 80% threshold and offer "raise the down payment" in the C1 band
     only. Comparing the ratio to the threshold in the UI would be eligibility logic.
  2. The down payment percentage should be available whenever the price and down payment are
     valid, so "x% af kaupverði" never disappears. Two parts differ. With the down payment at or
     above the price, or another value out of bounds, the engine answers `InvalidRequest`, which
     could carry the figures. With an empty field, such as the car age just after switching to used,
     the UI sends no request at all, because `LoanRequest` cannot carry an empty value. That needs
     either a separate figures call on the contract or a request that allows a missing value.


- **Story 12, car age counting.** `lykill.md` §4.3 does not say what the car's age is counted
  from (first registration or model year) or how part years round. That decides C1/C2 near a
  limit. It belongs in `lykill.md`, then in the field's help text.
- **Story 15, criteria form.** The stories use "WHEN … SHALL" without a starting state, while
  `definition-of-done.md` asks for Given/When/Then. The settlement above stands in for the
  missing scenarios until the story file is updated.
