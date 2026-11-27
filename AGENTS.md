# Agent rules for inventory-lab

This is an experiment of [my-factory](https://github.com/Henry-cmd325/my-factory). Follow the factory skills; when a skill and this file disagree, the skill wins, and note the conflict in `EXPERIMENTS.md`.

## Defaults

- .NET backend, AOT-first: no reflection-based code that breaks trimming or Native AOT.
- Clean Architecture + Vertical Slice Architecture; endpoints with [FastSharp](https://github.com/Henry-cmd325/FastSharp).
- The C# domain classes are the documentation. Do not write separate spec documents.
- Every slice gets property-based tests for its rules; mutation testing checks those tests.
- Everything runs in Docker: the app, the database and the tests.
- Use [Conventional Commits](https://www.conventionalcommits.org/); `CHANGELOG.md` is built from them.

## After each run

Add a row to `EXPERIMENTS.md` with the measured results, including the number of human corrections.
