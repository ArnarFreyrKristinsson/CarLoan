# Mutation samples

The one recurring manual job in the mutation process. About once a month
`tools/mutation/Get-Sample.ps1` appends a random handful of excused mutants and
agent-written tests below. A human reads them and writes the result. It works like testing
drinking water: nobody checks every glass, a random sample shows when something has gone wrong.

## How to check a sample

Tick each item you checked, then replace `_not checked yet_` with **pass** or **fail**.

An item fails when:

- an excuse's argument is wrong: some valid input does tell the two versions apart, or
- a test's expected value does not trace back to the lender's terms in `docs/rules/`: it
  was read off the engine's output instead.

## When a sample fails

1. Fix the item: remove the wrong excuse comment, or correct or delete the test.
2. Double `sampleSize` in `tools/mutation/settings.json` for the next sample.
3. Two failed samples in a row: set `autoExcuseEquivalent` to `false` in the same file.
   From then on "equivalent" verdicts are parked instead of excused, until you know why the
   judge went wrong. Set both values back once a sample passes again.

A sample this small will not catch the judge's ordinary few-percent error rate, and is not
meant to. It catches the judge or the test writer drifting.

## Sample 2026-10-11

Result: _not checked yet_

Excused mutants. Is the argument right, that no valid input can tell the two versions apart?

- [ ] CarLoan.Application/Mapping/LoanRequestMapper.cs:14 Statement mutation: ArgumentOutOfRangeException.ThrowIfNegative(request.CarAgeInYears); -> ;
      Excuse: the guard is a no-op for any CarAgeInYears >= 0, and a negative age still throws ArgumentOutOfRangeException from Car's constructor (Guard.NonNegative), so only the exception's ParamName and message wording differ.

Agent-written tests. Does each expected value trace back to the lender's terms in docs/rules/?

- none yet
