# UX Findings

What we have learned about real CarLoan users. This file grows; `docs/ux-rules.md` does not.

**Findings here outrank the general rules in `docs/ux-rules.md` when the two conflict.** A
rule is a reasonable default; a finding is evidence about the people actually using this.

## Status

No findings yet. No think-aloud test has been run, and there are no personas. Until this
file has content, builder and reviewer work from the rules alone.

## Format

One entry per finding. Keep them short and concrete.

```
### <short title>
- **Observed:** what the person did or said, not what it means.
- **Source:** think-aloud round 1 / support question / analytics / your own observation, with date.
- **Implication:** what the UI should do differently, if clear. Leave blank if it isn't.
- **Rules touched:** IDs from docs/ux-rules.md, if any.
```

Example of the shape (not a real finding):

```
### Interest rate field goes unnoticed
- **Observed:** 3 of 5 participants scrolled past the rate and compared monthly payments only.
- **Source:** think-aloud round 1, 2026-11-14.
- **Implication:** rate belongs in the comparison row, not below it.
- **Rules touched:** N6, S8.
```

## Personas

None written yet. When they exist they go here, grounded in the findings above rather than
invented.
