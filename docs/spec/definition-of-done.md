# Definition of Done

The finish line both builders and both reviewers work to. It carries the standing
requirements so individual stories can stay short.

## Every story

- Builds with zero warnings. A warning may stay only as a deliberate exception: suppressed
  at the line it applies to (`#pragma warning disable` or `[SuppressMessage]`) with a
  written justification.
- The CI checks pass locally: `dotnet format --verify-no-changes`,
  `dotnet build --configuration Release` and `dotnet test --configuration Release`.
- Nothing was merged by loosening a test, a threshold or a spec to make it pass.

## Engine story (Domain, Application)

Done when:

1. Its acceptance criteria pass as automated xUnit tests.
2. Every path has its own test.
3. The behaviour matches the lender specs in `docs/rules/` that it touches (currently only
   `lykill.md`), including the edge cases each spec states. Where code and spec disagree,
   the spec wins.
4. `engine-reviewer` reports no findings (see **Stop rule for review loops**).

## UI story

Done when:

1. Its acceptance criteria (Given/When/Then) are satisfied — checked against the built
   screen in the builder's self-check and by `ux-reviewer`, **not by an automated test
   suite.**
2. `docs/ui-plans/<screen>.md` exists, with a line for every rule ID in `docs/ux-rules.md`.
3. The builder's self-check against those rules is recorded in the plan file and its
   violations fixed.
4. `ux-reviewer` reports no findings (see **Stop rule for review loops**).

## Stop rule for review loops

Keep going until the reviewer reports no findings at all, or until three rounds are done,
whichever comes first. Then report the result: `No findings`, or every finding that
remains, most severe first. The builder may decline a fix that would violate another rule,
with the reason written in the plan file; a declined finding still counts as remaining.

Severity orders the findings, both reviewers: **1** cosmetic · **2** minor, user works
around it · **3** serious, user struggles or is misled · **4** blocks the task or gives a
wrong answer.
