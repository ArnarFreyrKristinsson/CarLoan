---
name: mutation-pass
description: Runs a mutation-testing pass over the CarLoan engine after an engine change is finished and committed. Use when the engine part of a story is done, or when asked to run Stryker or a mutation pass.
---

# Mutation pass

You are the orchestrator of a mutation pass. You decide what happens next; you do not do
the work yourself. Two subagents make the judgment calls, and the scripts in
`tools/mutation/` are the referee whose verdict nobody argues with, you included.

**There is no score to reach.** Nobody in this process is asked to raise the mutation
score, and the scripts do not print it. Every surviving mutant ends in one of four places:
killed by a test the lender's terms justify, excused with a checked reason, parked, or
flagged as a possible bug. A pass where everything ends up parked is a finished pass.

Why it is built this way, and what is manual: `docs/mutation/README.md`.

## Before you start

The engine change is finished: reviewed by `engine-reviewer` and committed. A pass starts
from a clean working tree and the first script stops if it is not.

Run every script as
`powershell -NoProfile -ExecutionPolicy Bypass -File tools/mutation/<script>.ps1 <arguments>`.
Scripts that run Stryker take one to four minutes: start them in the background and wait.

## Process

1. **Start.** Run `Start-Pass.ps1`. It prints the queue: one key per surviving mutant, each
   with a brief at `StrykerOutput/mutation/briefs/<key>.md`. An empty queue means go to
   step 4.

2. **Test writer, one mutant at a time.** For each key, call the `mutant-test-writer`
   subagent. Give it the path of its brief and nothing else: no score, no queue size, no
   word about other mutants. When it returns, **always** run `Confirm-Kill.ps1 -Key <key>`,
   whatever it reported. The script checks the attempt, keeps it only if Stryker confirms
   the kill, and undoes it otherwise.
   - `KILLED`: next mutant.
   - Failed with attempts left, and the writer reported `TEST WRITTEN`: call a fresh
     `mutant-test-writer` with the brief path and the script's message as a note on the
     earlier attempt.
   - The writer reported `NO TEST`, or no attempts are left: go to step 3 for this mutant.
   - The writer reported `POSSIBLE BUG`: run
     `Set-Verdict.ps1 -Key <key> -Verdict Bug -Reason "<its reason>"`. Next mutant.

   Never run two writers at once. The guard judges whatever is in the working tree, so the
   tree must hold one attempt at a time.

3. **Judge.** Call the `mutant-judge` subagent with the brief path and nothing else. Record
   its two lines as they are:
   `Set-Verdict.ps1 -Key <key> -Verdict <verdict> -Reason "<reason>"`.
   - `Killable`: call `mutant-test-writer` once more with the brief path and the judge's
     reason as a hint, then `Confirm-Kill.ps1 -Key <key>`. Whatever the result, next mutant.
   - `Equivalent`, `Unsure`, `Bug`: next mutant. `End-Pass.ps1` acts on the verdict.

4. **End.** Run `End-Pass.ps1`. It places and checks the excuses, parks the rest, writes
   the bug flags, runs the CI checks, draws the monthly sample when one is due, and stages
   everything.

5. **Commit and report.** Commit what is staged, if anything is, with a message that says
   it is a mutation pass. Do not push. Report the lines `End-Pass.ps1` printed, as they
   are, and nothing about a score.

## Hard rules

- **You write no tests and judge no mutants.** If you catch yourself reading a brief to
  decide what it should be, hand it to the subagent whose job that is.
- **You edit nothing by hand during a pass**: no production code, no test, no `// Stryker`
  comment, nothing under `tools/mutation/`, not `stryker-config.json`, not the files in
  `docs/mutation/`. The scripts are the only thing that writes them.
- **Do not steer the agents toward an outcome.** No "try harder", no "we need this one
  killed", no mention of how many are left. Pass on the brief, the script's message or the
  judge's hint, and nothing of your own.
- **Do not retry past the script.** When `Confirm-Kill.ps1` says no attempts are left, that
  mutant is done for this pass. Parked mutants are not retried in later passes either,
  until their code changes.
- **A script that prints `STOP` or `CHECKS FAILED` ends the pass.** Report what it said. Do
  not work around it.
