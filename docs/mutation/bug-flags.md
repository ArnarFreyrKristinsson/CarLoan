# Possible bugs found by mutation testing

Written by `tools/mutation/End-Pass.ps1`. An entry lands here when an agent found a mutant
whose behaviour matches the lender's terms **better** than the real code does. That is not a
gap in the tests; it may be a wrong number shown to a user. No test is written for it, because
a test would lock the wrong behaviour in.

Reading these is optional and unscheduled. For each one: compare the real code with
`docs/rules/`, fix the code through the `engine-building` skill if the flag is right, and tick
the box either way.
