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

## Run it

Only Docker is needed.

| Goal | Command |
| --- | --- |
| Run the API on http://localhost:8080 | `docker compose up --build api` |
| All tests (agent loop and PR tier) | `docker compose run --rm test` |
| Mutation testing (domain) | `docker compose run --rm mutation` |
| Stop and delete the data | `docker compose down -v` |

```bash
curl -X POST localhost:8080/stock/ABC-1/receipts -H 'content-type: application/json' -d '{"quantity":5}'
curl -X POST localhost:8080/stock/ABC-1/issues   -H 'content-type: application/json' -d '{"quantity":9}'   # 409: only 5 on hand
```

## Layout

| Path | What lives there |
| --- | --- |
| `src/Inventory.Domain/` | The domain classes: the documentation of the business rules |
| `src/Inventory.Api/Features/<Area>/<Slice>/` | One folder per use case: endpoint, handler and store |
| `tests/` | Property-based domain tests, HTTP tests against PostgreSQL, architecture tests |
| `db/init.sql` | Database schema |
| `.claude/skills/` | The factory skills used for the runs (see `FACTORY_VERSION`) |

## Status

Run 1 built the first feature: stock receipts and issues, with the rule "stock can never go negative". Results are in [`EXPERIMENTS.md`](EXPERIMENTS.md).

## License

[Apache License 2.0](LICENSE)
