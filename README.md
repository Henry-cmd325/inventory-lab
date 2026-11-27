# inventory-lab

The first experiment of [my-factory](https://github.com/Henry-cmd325/my-factory): can a coding agent, guided only by the factory skills, build an inventory system that passes the factory's quality gates?

> This repository is disposable. The factory will change after each experiment, and this code may be thrown away and rebuilt.

## The experiment

An agent (Claude Code first) gets plain-language inventory requirements and the factory skills, and nothing else. It writes the C# domain classes, the vertical slices, the property-based tests and the Playwright UI scripts.

We measure, for each run:

| Measure | Why |
| --- | --- |
| Builds with AOT analyzers on | AOT-first is a factory default |
| Tests pass | Basic correctness |
| Mutation score (Stryker.NET) | Do the tests really catch bugs? |
| Time from request to green | The "insane velocity" goal |
| Human corrections needed | How good the skills are |

Results go into [`EXPERIMENTS.md`](EXPERIMENTS.md). Quality-gate thresholds will be chosen from these numbers.

## Stack

.NET (AOT-first), Clean Architecture + Vertical Slice Architecture, [FastSharp](https://github.com/Henry-cmd325/FastSharp), everything in Docker.

## Status

No application code yet. The agent writes it during the first run.
