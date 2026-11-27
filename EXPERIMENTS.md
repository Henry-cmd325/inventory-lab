# Experiment log

One row per run, newest first. "Corrections" counts how many times a human had to step in.

| Date | Run | Agent | Factory version | Feature | AOT build | Tests | Mutation score | Time to green | Corrections | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 2026-11-27 | 1 | Claude Code (cloud session) | 70de404, updated to 1212bcc during the run | Stock movements: `ReceiveStock` and `IssueStock` slices, rule "stock can never go negative" | Yes: 0 warnings, 12.5 MB binary, 38.8 MB image, about 0.4 s to first response, about 11 MiB RAM | 44 pass (30 domain, 11 API, 3 architecture), 12 s in Docker | 100% on the domain (32 of 32); 80.6% on the first correctly wired run | 16 min | 0 | Same session wrote the skills and the code. See [Run 1 notes](#run-1-notes). |

## Run 1 notes

**Setup.** .NET 10 SDK (`10.0.404`), PostgreSQL 17, CsCheck 5.0.1, xUnit v3, Stryker.NET 5.1.0, all in Docker Compose. The skills from [my-factory](https://github.com/Henry-cmd325/my-factory/tree/main/skills) are copied into `.claude/skills/`.

**Timeline (UTC).** 22:32 start. 22:37 first five skills written. 22:41 first green test run. 22:44 Native AOT image builds and answers requests. 22:48 mutation score 100%.

**What went wrong, and what the agent changed (no human corrections):**

1. **Native AOT publish failed although `dotnet build` was clean.** Dapper.AOT 1.1.0 intercepted all 5 Dapper calls, but ILC still reported 20+ trimming and AOT warnings from `Dapper.dll`. The stores now use plain Npgsql commands. Skill change: publish is the real AOT check; no Dapper.
2. **A property found a real bug.** The model-based HTTP property sent an invalid quantity to an unknown SKU and got 404 instead of 400, because the handler loaded the item before validating the quantity. Fix: a `Quantity` value object, parsed before the first store call. The failing seed is kept as a regression test. Skill change: parse every input first.
3. **Mutation testing reported 0% at first.** With xUnit v3, Stryker's default `vstest` runner could not capture coverage and marked all 31 mutants as survived. With `"test-runner": "mtp"` the score was 80.6%. Skill change: use `mtp`, and never record a run where every mutant survived.
4. **Surviving mutants were all error messages** (plus one equivalent mutant in `Sku.Parse`). Tests now assert the messages, and `Sku.Parse` was simplified. Score: 100%.
5. Two small compile errors from guessing the CsCheck API. Skill change: an API reminder in `property-testing`.

**Conflict with `AGENTS.md`.** `AGENTS.md` says "endpoints with FastSharp". The `vertical-slice` skill says not to reference FastSharp until it publishes without trimming warnings: `FastSharp.Modules 1.0.0-beta.16` with `PublishAot` gives IL2104 (FastSharp, FluentValidation) and IL2062/IL2072 (from its generated registry), and it pulls in EF Core, Mapster and FluentValidation. The skill wins, so the slices follow FastSharp's shape (one route group per area, one class per endpoint) by hand.

**Limits of this run.**

- The same agent session wrote the skills and then the code, so it knew more than the skills say. Run 2 should start a fresh session with only the skills and the requirement.
- Mutation testing covers the domain project only. Handlers and stores are covered by HTTP tests but not mutated yet.
- The timings come from a cloud container with warm caches after the first restore; the first restore took about 12 s.
- No frontend yet, so no Playwright scripts.
