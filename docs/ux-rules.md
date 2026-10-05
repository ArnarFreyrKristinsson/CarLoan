# UX Rules

The usability rules every UI decision in CarLoan must satisfy. Written down once so the
builder and the reviewer judge by identical text instead of each recalling its own version.

These are non-functional requirements: they apply to every UI story, so they are not
repeated in the stories themselves. See `docs/spec/definition-of-done.md`.

Cite rules by ID (`N4`, `S8`) in plan files, self-checks, review findings and commit
messages.

## Nielsen's usability heuristics

| ID | Rule |
|---|---|
| N1 | **Visibility of system status** — users always know what the system is doing, through prompt feedback. |
| N2 | **Match with the real world** — speak in terms users know, not internal jargon; order information as the real world does. |
| N3 | **User control and freedom** — clearly marked ways out of unwanted states, plus undo and redo. |
| N4 | **Consistency and standards** — the same word or action always means the same thing; follow platform conventions. |
| N5 | **Error prevention** — remove conditions that invite errors, or confirm before users commit. |
| N6 | **Recognition rather than recall** — keep options and needed information visible instead of making users remember them. |
| N7 | **Flexibility and efficiency** — accelerators for experienced users that novices don't have to notice. |
| N8 | **Aesthetic and minimalist design** — leave out what is irrelevant or rarely needed; every extra element takes visibility from what matters. |
| N9 | **Error recovery** — plain-language error messages, no codes, saying exactly what went wrong and how to fix it. |
| N10 | **Help and documentation** — ideally not needed; where it is, easy to find, task-focused, with concrete steps. |

## Shneiderman's Eight Golden Rules

| ID | Rule |
|---|---|
| S1 | **Consistency** — same action sequences, terminology, layout, colors and fonts in similar situations; exceptions few and understandable. |
| S2 | **Universal usability** — design for novices and experts, different ages, abilities, languages and devices. |
| S3 | **Informative feedback** — every action gets feedback, modest for frequent minor actions, substantial for rare major ones. |
| S4 | **Closure** — action sequences have a clear beginning, middle and end, with confirmation on completion. |
| S5 | **Prevent errors** — make serious errors impossible where possible; when one happens, give simple, specific recovery steps. |
| S6 | **Easy reversal** — make actions reversible wherever possible. |
| S7 | **User in control** — users initiate actions and the interface responds; no surprises or changes in familiar behavior. |
| S8 | **Reduce short-term memory load** — never make users carry information from one display to use on another. |

## Overlaps

These pairs say the same thing from two sources. One issue, cite both IDs; do not report it
twice.

S1/N4 · S2/N7 · S3/N1 · S5/N5+N9 · S6/N3 · S7/N3 · S8/N6

## Conflicts

The rules can pull against each other — N7 (accelerators for experts) against N8
(minimalist design) is the usual one. When satisfying one rule would violate another, the
builder does not make the change: it records the conflict in the screen's plan file, marked
for human decision. Neither side resolves a conflict silently.

## Supporting findings

Not binding rules, but established findings the builder and reviewer should weigh:

- **Fitts's law** — bigger and closer targets are faster to hit. Primary actions get size
  and proximity; destructive ones do not sit next to them.
- **Nielsen's "5 users" finding** — a think-aloud test with about five people uncovers most
  major problems, so several small rounds beat one big one. This is why the reviewer is a
  first pass, not a substitute for testing with people.

## Changing this file

Rules are added when a review or a think-aloud test keeps surfacing the same problem, and
when a general rule is not enough to prevent it. Observations about real users do not
belong here — they go in `docs/ux-findings.md`, which outranks this file when the two
conflict.
