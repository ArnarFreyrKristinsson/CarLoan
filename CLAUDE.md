# CarLoan — Orchestrator

This file routes work to the right role. The detailed rules live with the role that needs
them, so a session loads what the task requires and nothing else.

## Pick a role first

| Working on | Load | Reads |
|---|---|---|
| `CarLoan.Domain`, `CarLoan.Application`, their tests | skill `engine-building` | `docs/rules/lykill.md`, `docs/api-contract.md`, `docs/architecture.md` |
| Any UI screen or UI change | skill `ui-building` | `docs/ux-rules.md`, `docs/ux-findings.md`, `docs/api-contract.md`, the story |
| Reviewing engine code | subagent `engine-reviewer` | spec, contract, code, tests |
| Reviewing a UI screen or a user story | subagent `ux-reviewer` | `docs/ux-rules.md`, `docs/ux-findings.md`, the plan file, UI code |

Builders are **sessions with a skill**, not subagents: you need to steer a builder while it
works and see its reasoning. Reviewers are **subagents** because fresh eyes are the point —
a reviewer that watched the screen get built inherits its assumptions, which is why
heuristic evaluation is never done by the designer.

Load the skill before writing code, not after. If a task spans both layers, do the engine
part and the UI part as separate passes with the matching skill loaded.

## Always, whichever role

- **.NET 8 / C# 12.** File-scoped namespaces, namespaces matching folders, `record` for
  immutable data, primary constructors where they fit, `_camelCase` private fields with no
  `this.`, `Nullable` and `ImplicitUsings` on.
- **SOLID**, in full, as the `engine-building` skill states it.
- **The UI never does loan math.** Rates, fees, payments, LTV and eligibility are the
  engine's, reached through `docs/api-contract.md`. Not even a subtraction for display.
- **`docs/rules/lykill.md` overrides the code.** Where they disagree, the code is the bug.
- **Nothing that defines "correct" gets edited to make work pass** — not the spec, the API
  contract, the UX rules, the tests or the thresholds. Raise the conflict instead.

## Testing policy

- **Engine code is tested, strictly TDD**, Red → Green → Refactor, one test per path, no
  production code without a failing test first. Details and test-style rules:
  `.claude/skills/engine-building/SKILL.md`.
- **The UI is not tested automatically.** Deliberate decision. Do not add UI test projects,
  snapshot tests or browser automation.
- The consequence: **no logic in the UI.** Anything worth a test lives behind the contract
  in a tested layer. Wanting a test for UI code means that code is in the wrong layer.

## Done means

`docs/spec/definition-of-done.md`. In short: acceptance criteria met, the CI checks
passing locally, and the matching reviewer reporting no findings within 3 review rounds.

## Where things live

```
CLAUDE.md                        this file — routing and always-on rules
.claude/
  agents/ux-reviewer.md          read-only UI + story reviewer
  agents/engine-reviewer.md      read-only engine reviewer
  skills/ui-building/SKILL.md    how to build UI
  skills/engine-building/SKILL.md  how to build the engine
  settings.json                  permissions and hooks (not set up yet)
docs/
  rules/lykill.md                the lender spec — defines correct
  api-contract.md                the UI ↔ engine boundary
  ux-rules.md                    N1–N10, S1–S8 — binding UI rules
  ux-findings.md                 what we learn about real users; outranks ux-rules.md
  ui-plans/<screen>.md           one rule plan per screen, written before building
  spec/definition-of-done.md     the finish line
  architecture.md                patterns in the engine
CarLoan.Domain/ .Application/    the code
CarLoan.*.Tests/                 xUnit tests
stryker-config.json              mutation threshold
.github/workflows/               CI
```

The split is by who the file is for: `.claude/` is how the AI may work, `docs/` is what
"correct" means and is read by both of us, tests and CI enforce it regardless.

## Guardrails

Guidance lives in these instruction files, which is the layer that drifts. The rules worth
enforcing harder belong further from the AI's reach — tool access in the agent files,
permissions and hooks in `.claude/settings.json`, NetArchTest and Stryker thresholds in the
test projects, branch protection on GitHub. One principle holds across all of them: the AI
does not edit its own guardrails, and does not push or deploy.
