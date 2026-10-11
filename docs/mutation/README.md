# Mutation testing

How Stryker is used on the engine (`CarLoan.Domain` and `CarLoan.Application`), and why it
is set up the way it is.

## The goal

Mutation testing should find tests the lender's terms call for and that nobody wrote. It
should not turn into an AI chasing 100% for the sake of 100%.

A 100% target cannot be met honestly: some mutants change the code without changing what it
does, so no test can ever kill them. An agent given an impossible target, with nobody
watching, does not reliably stop and say so. It reaches the number another way: suppression
comments, a loosened config, tests that dig into private state, or rewritten production
code. All of those look tidy in a diff, and the score stops meaning anything.

So the process is built on one rule: **nothing is asked to raise the score.** Stryker's
`break` threshold is 0, so no build fails on it. The agents never see it. The scripts never
print it. It is a number a human reads in the HTML report.

In its place: **no survivor is left unexplained.** Each surviving mutant ends in exactly
one place:

| Ends as | Meaning |
|---|---|
| Killed | A test was added, and its expected value comes from the lender's terms. |
| Excused | Judged equivalent. A `// Stryker disable once` comment with the reason sits above it. |
| Parked | Not killed, not excused. Stays a survivor in the report and is not retried until its code changes. |
| Bug flag | The mutated code fits the lender's terms better than the real code. Written to `bug-flags.md`. |

Parked is a normal ending. The suite does not have to be perfect, only clearly better than
it would be with no mutation testing, and honest about what is left.

## How a pass runs

The `mutation-pass` skill runs it, after an engine change is finished and committed.

1. `Start-Pass.ps1` runs Stryker on what changed since `master` (everything, if the last
   full run is more than a week old) and lists the survivors.
2. For each survivor the `mutant-test-writer` subagent is asked one thing: does this change
   break something the terms require, and if so, prove it with a test.
3. `Confirm-Kill.ps1` checks the attempt and keeps it only if Stryker confirms the kill.
   Two attempts per mutant.
4. Survivors that are left go to the `mutant-judge` subagent: killable (with a concrete
   input, which gives the writer one more attempt), equivalent (with the reason), unsure,
   or a possible bug.
5. `End-Pass.ps1` places the excuses and checks each one against Stryker, parks the rest,
   writes bug flags, runs the CI checks, draws the sample when due and stages the result.

## What each piece is there to prevent

| Piece | Prevents |
|---|---|
| No score target, `break: 0`, score never shown to agents | The impossible goal that invites gaming. |
| `ignore-methods` in `stryker-config.json` | Tests that pin exception-message wording nobody depends on. One decision, made once, instead of an agent judging each mutant. |
| Writer and judge are separate subagents | An agent grading its own work. The one that would benefit from "equivalent" is not the one who decides it. |
| The judge is read-only and gets only the code, the mutation and the terms | A low score, or the effort already spent, making "equivalent" more tempting. |
| "Unsure" is parked, never excused | Silent skips. Only a confident, argued "equivalent" becomes an excuse. |
| The guard (`Test-WriterDiff.ps1`) checks the result, not the tool calls | The writer touching production code, config, scripts or Stryker comments, using reflection or mocks, or rewriting existing tests. It holds however the edit was made. |
| Stryker confirms every kill; tests run three times on the real code | Taking the agent's word for it, and flaky tests. |
| Attempts are counted by a script | Grinding away at a mutant that cannot be killed. |
| Expected values must come from `docs/rules/` | Tests that freeze today's output, bugs included. |
| A script places each excuse and re-runs Stryker to check it covers that one mutant only | A misplaced comment hiding mutants nobody judged. A comment that covers anything else is taken out again. |
| An excuse is removed when the line under it changes | An old excuse quietly covering new code. |
| The bug flag | Pinning a wrong number with a test, where it would then look verified. |
| The random monthly sample and the pass log | Slow drift that no single pass shows. |

## What is automatic and what is manual

**Automatic, every pass:** running Stryker, finding survivors, writing tests, checking and
confirming them, classifying leftovers, placing and checking excuses, removing stale
excuses, parking, flagging possible bugs, the CI checks, the pass log, drawing the sample.

**Manual:**

| What | When | Where |
|---|---|---|
| Reviewing the setup itself: the scripts, the two agent files, the skill, the config | Once, and again when one of them changes | `tools/mutation/`, `.claude/agents/mutant-*.md`, `.claude/skills/mutation-pass/` |
| Deciding a category of mutant is noise | When a new pattern shows up, for example if the engine starts logging | `ignore-methods` in `stryker-config.json` |
| Checking the sample | About monthly, when `End-Pass.ps1` says `SAMPLE DUE` | `docs/mutation/samples.md` |
| Reading bug flags | Whenever you like; they are rare | `docs/mutation/bug-flags.md` |
| Reading the score and the excuse trend | Whenever you like | The HTML report, `docs/mutation/log.md` |

Not automated on purpose: pushing and merging, and any change to the rules of the process.

## What we chose to accept

- **About one excuse in thirty may be wrong.** That is the error rate measured for
  confident "equivalent" calls in Meta's study, on other code and an older model. Excuses
  are trusted without a human reading each one; the sample is what notices if the real rate
  here is much worse.
- **Tests still describe the engine as it is.** The bug flag catches some existing bugs,
  not all.
- **The referee runs on the same machine as the agents.** A script cannot be argued with,
  but an agent with a shell that set out to defeat it could. Nothing in the process gives
  one a reason to, and what it would have to leave behind shows in the diff and the sample.

## Settings

`tools/mutation/settings.json`. Change them by hand; an agent does not.

| Setting | Meaning |
|---|---|
| `sinceTarget` | The branch a normal pass compares against. |
| `fullRunMaxAgeDays` | A pass mutates everything when the last full run is older than this. |
| `attemptsPerMutant`, `hintedAttempts` | Tries the writer gets, and extra tries after a "killable" verdict. |
| `testRepeats` | How many times new tests must pass on the real code. |
| `autoExcuseEquivalent` | `false` parks "equivalent" verdicts instead of excusing them. See `samples.md`. |
| `sampleSize`, `sampleIntervalDays` | How many of each kind the sample draws, and how often. |

## Files

```
stryker-config.json            what Stryker ignores; break threshold 0
tools/mutation/*.ps1           the referee scripts
tools/mutation/settings.json   the settings above
tools/mutation/state/          excuses, parked mutants and agent-written tests, as JSON
docs/mutation/log.md           one line per pass
docs/mutation/samples.md       the samples and how to check them
docs/mutation/bug-flags.md     possible bugs
StrykerOutput/mutation/        one pass's working files and reports (not in git)
```

Sources for the reasoning: Google's "Practical Mutation Testing at Scale", Meta's
"Mutation-Guided LLM-based Test Generation at Meta" (FSE 2025), "Large Language Models for
Equivalent Mutant Detection" (ISSTA 2024), and ImpossibleBench (2025).
