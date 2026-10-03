# Definition of Done

The finish line both builders and both reviewers work to. It carries the standing
requirements so individual stories can stay short.

## Every story

- Builds with no new warnings.
- CI is green: tests, coverage and Stryker thresholds (`stryker-config.json`).
- Nothing was merged by loosening a test, a threshold or a spec to make it pass.

## Engine story (Domain, Application)

Done when:

1. Its acceptance criteria pass as automated xUnit tests.
2. Every path has its own test, written before the production code (see
   `.claude/skills/engine-building/SKILL.md`).
3. The behaviour matches `docs/rules/lykill.md`, including the edge cases that spec states.
   Where code and spec disagree, the spec wins.
4. `engine-reviewer` reports no severity 3–4 findings.

## UI story

Done when:

1. Its acceptance criteria (Given/When/Then) are satisfied — **verified by hand, not by an
   automated test suite.** See "Why the UI is not tested automatically" below.
2. `docs/ui-plans/<screen>.md` exists, with a line for every rule ID in `docs/ux-rules.md`.
3. The builder's self-check against those rules is recorded in the plan file and its
   violations fixed.
4. `ux-reviewer` reports no severity 3–4 findings, after at most 3 rounds.
5. Any rule conflict or leftover finding is written up in the plan file for human decision.

## Why the UI is not tested automatically

Deliberate decision, not an omission. UI tests at this stage would pin down markup that is
still changing, and the properties worth checking — whether a person can tell what the
system is doing, whether an error message helps — are the ones automated UI tests check
worst. Usability is verified by the rule plan, the self-check, the `ux-reviewer` loop and
later by think-aloud tests with real people.

This holds only for presentation. Anything the UI calls, any mapping, formatting or
decision logic, belongs behind the API contract in a tested layer — which is also why
**the UI never does loan math** (`CLAUDE.md`). A UI story that would need a test is a UI
story with logic in the wrong layer.

## Stop rule for review loops

Stop when no severity 3–4 findings remain, or after three rounds, whichever comes first.
Round 3 leftovers come to the human. The builder may decline a fix that would violate
another rule, with the reason written in the plan file.

Severity scale, both reviewers: **1** cosmetic · **2** minor, user works around it ·
**3** serious, user struggles or is misled · **4** blocks the task or gives a wrong answer.
