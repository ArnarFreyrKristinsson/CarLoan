# UI Plans

One file per screen, named after the screen: `docs/ui-plans/<screen>.md`.

The builder writes the plan **before** building the screen and updates it after the
self-check and each review round. It is where you can see what the UI decided and why
without reading the code.

No plan files exist yet — the first one is written when the first screen is built.

## Template

```markdown
# <Screen name>

**Story:** link or ID from docs/spec/
**Status:** planned | built | in review (round N) | done | blocked on human decision

## Rule plan

One line per rule in docs/ux-rules.md: how this screen satisfies it, or why it does not
apply. Every ID N1–N10 and S1–S8 appears exactly once. Overlapping pairs may share a line.

| ID | How this screen satisfies it, or why it doesn't apply |
|---|---|
| N1 | |
| … | |

## Findings applied

Which entries from docs/ux-findings.md shaped this screen, and how. "None yet" is a valid
answer while that file is empty.

## Self-check

Date, and the result per rule that needed a fix: what was wrong, what changed.

## Review rounds

### Round N — <date>
| Finding | Rule | Severity | Action |
|---|---|---|---|
| | | | fixed / declined (conflict) / for human |

## For human decision

Rule conflicts the builder declined to resolve, and anything left after the last round.
State the finding, both rules involved, and the reasoning. Empty is the normal case.
```
