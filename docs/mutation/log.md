# Mutation pass log

One line per pass that had survivors to work through, appended by
`tools/mutation/End-Pass.ps1`. Nothing here is a target. The
column to watch is the standing total of excuses: true equivalents should be rare in
`CarLoan.Domain` and somewhat more common in `CarLoan.Application`. A total that keeps
climbing means the judge is excusing more than this kind of code should produce.

| Date | Mode | Queue | Killed | Excused | Parked | Bug flags | Standing totals |
|---|---|---|---|---|---|---|---|
| 2026-10-11 | full | 2 | 0 | 1 | 1 | 0 | CarLoan.Application: 1 excused, 0 parked; CarLoan.Domain: 0 excused, 1 parked |
