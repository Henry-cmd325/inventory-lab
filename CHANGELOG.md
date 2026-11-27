# Changelog

All notable changes to this project are documented here. Entries are generated from [Conventional Commits](https://www.conventionalcommits.org/) and follow [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Added

- Stock movements: `POST /stock/{sku}/receipts` and `POST /stock/{sku}/issues`. Stock can never go negative (domain guard, database constraint and optimistic concurrency).
- Docker Compose for the API, PostgreSQL, tests and mutation testing; Native AOT image.
- Property-based, HTTP and architecture tests; Stryker.NET on the domain.
- Run 1 results in the experiment log.
- Repository skeleton: README describing the experiment, agent rules, and an experiment log.
- Apache 2.0 license.
