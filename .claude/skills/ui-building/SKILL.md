---
name: ui-building
description: Rules and process for building or changing CarLoan UI screens. Use for any UI work — new screens, changes to existing ones, and fixing ux-reviewer findings.
---

# UI building

You are the UI builder. You build presentation only. The engine owns every number.

## Read first

1. `docs/ux-rules.md` — the binding rules, N1–N10 and S1–S8. Work from that text, not from
   your own recall of Nielsen or Shneiderman: the reviewer reads the same file, and the loop
   only converges if you both judge by identical wording.
2. `docs/ux-findings.md` — what we know about real users. **Findings outrank the general
   rules when they conflict.**
3. `docs/api-contract.md` — the shape you call and the fields you may show. If it is still
   marked undecided for what you need, stop and settle it with the human first. Do not guess
   a field or a unit.
4. The story in `docs/spec/` and `docs/spec/definition-of-done.md`.

## Process

Follow this for every screen. The per-screen enumeration of the rules is yours to generate,
not the human's to write.

1. **Plan.** Write `docs/ui-plans/<screen>.md` from the template in
   `docs/ui-plans/README.md`: one line per rule ID on how this screen satisfies it, or why
   it does not apply. Every ID appears exactly once. Do this *before* building — it is what
   puts the rules in focus at the moment the decisions are made.
2. **Build** the screen to the plan.
3. **Self-check.** Go back through every rule ID against the screen you actually built, not
   against the plan. Fix the violations before review, and record what changed in the plan
   file.
4. **Review.** Call the `ux-reviewer` subagent. Fix each finding and cite its rule ID in the
   commit message.
5. **Repeat** from step 3 until the reviewer reports no severity 3–4 findings, at most 3
   rounds. Leftovers after round 3 go to the human.
6. **Decline conflicts.** If a fix would violate another rule, do not make it. Record the
   finding, the conflicting rule and your reasoning in the plan file under "For human
   decision".

Why write the plan when you already know these rules: knowing is not applying. During a
build your focus is the screen, and the rules drift to the background as the work piles up.
The plan, the self-check and the reviewer bring the focus back at the three moments it
matters.

## Hard rules

- **The UI never does loan math.** No rate, fee, payment, LTV or eligibility calculation in
  UI code — not even a "quick" subtraction for display. Call the engine. If you need a
  derived value, the API contract gains a field.
- **No automated UI tests.** This is a deliberate project decision
  (`docs/spec/definition-of-done.md`), not something to fix. Correctness of the UI is
  established by the plan, the self-check, the reviewer and later by think-aloud tests.
  Do not add a UI test project, snapshot tests or browser automation.
- **Therefore: no logic in the UI.** Anything worth testing — mapping, formatting rules,
  decisions, validation beyond input constraints — belongs in a tested layer behind the
  contract. If you catch yourself wanting a test for UI code, that code is in the wrong
  place. Move it, and let its own layer's tests cover it (`engine-building` skill).
- **Do not edit the rules, the spec, the contract or the tests** to make your work fit.
  Those define correct. Raise the problem instead.

## Units and language

The user's terms, not the code's: amounts in kr with thousand separators, rates in %, terms
in months. Use the lender's vocabulary from `docs/rules/lykill.md` — purchase price, down
payment, loan amount, financing ratio — not variable names, and not an internal rule ID
shown raw to a user (N2).

## When a rule keeps getting flagged

If the reviewer reports the same kind of problem across screens, that is the signal to add a
rule to `docs/ux-rules.md` so the next build stops making it. Propose it to the human; do
not add it yourself mid-loop.
