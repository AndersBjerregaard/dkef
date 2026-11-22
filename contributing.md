# Contributing to dkef

This guide describes how agentic sessions should work towards a defined goal and contribute source-code changes for human review. Read the root [AGENTS.md](AGENTS.md) and any applicable package guide before working:

- [Frontend guidance](src/dkef-vue/AGENTS.md)
- [Backend guidance](src/dkef-api/AGENTS.md)

## 1. Establish the goal and scope

- Identify the requested outcome and acceptance criteria. Ask for clarification if an ambiguity affects behavior, safety, or scope.
- Inspect the relevant code, tests, configuration, and repository instructions before proposing changes. Follow existing structure, naming, and patterns.
- Outline a short plan for multi-step work and give concise updates for meaningful discoveries, tradeoffs, or blockers.
- Limit changes to the goal. Do not bundle unrelated refactoring, dependency upgrades, or formatting changes.
- Inspect `git status` before editing. Preserve existing user changes; do not reset, discard, or overwrite unrelated work. Never commit credentials, local configuration secrets, or imported data.

## 2. Work on a branch, never directly on the default branch

Discover the repository's current default branch rather than assuming `main` or `master`. Feature requests must always use a feature branch based on the updated default branch. The same branch-and-PR workflow applies to fixes and documentation changes.

For a clean working tree, a typical CLI setup from the repository root is:

```sh
gh repo view --json defaultBranchRef --jq '.defaultBranchRef.name'
git fetch origin
git switch -c feature/<short-goal> origin/<default-branch>
```

Replace the placeholders with the actual default branch and a descriptive branch name. Prefixes such as `fix/` or `docs/` are appropriate for those kinds of work. An existing task branch may be reused after verifying that it is based on the default branch and contains only relevant work. If the working tree contains unrelated changes, isolate the task safely (for example, in a separate worktree) or ask the user how to proceed.

Do not modify, commit to, or push directly to the default branch. Do not force-push or rewrite shared history without explicit authorization. Branch protections apply regardless of which tool is used.

## 3. Tool inventory and boundaries

These are the tools intended for agent sessions. Check availability, versions, and authentication before relying on them; report missing prerequisites instead of silently replacing the repository's toolchain.

| Tool | Intended use | Boundaries |
| --- | --- | --- |
| `dotnet` | Restore, build, test where tests exist, and run the ASP.NET Core API or .NET data utility. | The API targets `net10.0`. API startup automatically applies EF migrations; verify local configuration first. The data utility is interactive and can modify data. |
| Bun (`bun`) | Frontend dependencies, Vite development/builds, linting, formatting, and Vitest. | Prefer Bun instead of Node.js-based command workflows; do not use npm/yarn or introduce their lockfiles. Preserve the repository's Bun lockfile. |
| `python3` | Temporary agent scripting, inspection, or automation needed to accomplish the task. | Python is not used in application source. Do not introduce a Python application component, runtime dependency, or source-code implementation. |
| `dotnet ef` | Inspect EF Core migrations and generate migrations when a schema change requires them. | Specify the context and matching output directory. Review generated migrations and snapshots. Apply migrations only to a confirmed local development database; shared/production changes require explicit authorization. |
| Ansible (`ansible`, `ansible-playbook`) | Read and edit provisioning/deployment configuration; perform non-deploying static validation when available. | 🚨 Do not run playbooks directly, including with `--check`. Hand execution over to a human; do not treat deployment as contribution validation. |
| Docker (`docker`) | Build images and run explicitly needed local development dependencies. | Verify the active Docker context and targets. Do not deploy, publish images, prune unrelated resources, or remove volumes as part of routine validation. The only Compose stack is `src/dkef-data/db/docker-compose.yaml` (Postgres + MinIO). |
| GitHub CLI (`gh`) | Inspect repository metadata, issues, PRs, and checks; push-related GitHub workflow and PR creation when needed. | Use the authenticated account's existing permissions. Never bypass branch protections or modify the default branch directly. |
| GitHub MCP | Repository discovery, issue/PR context, and supported GitHub operations. | Prefer MCP when available. If an operation fails because MCP lacks authorization, fall back to GitHub CLI for that operation, subject to the same safeguards. |

Git remains the local tool for status, branches, diffs, staging, commits, and pushes. Use focused commands and stage specific task files rather than blindly staging the entire working tree.

### GitHub authorization fallback

1. Try the appropriate GitHub MCP operation when available.
2. If MCP lacks authorization, check the existing CLI authentication with `gh auth status`, then perform the equivalent operation with `gh` if authorized.
3. If MCP is unavailable or does not support the operation, the CLI may also be used.
4. If neither route is authorized, stop that operation and report the blocker and the next action needed from a human. Do not request broader access, change credentials, or circumvent repository protections just to finish the task.

Both routes must use the feature branch and target the default branch only through a PR. A tool fallback is not permission to commit directly, merge the PR, or deploy.

## 4. Implement and validate

- Make the smallest coherent change that satisfies the goal. Update relevant tests and documentation alongside behavior changes.
- Keep frontend user-facing text in Danish. Follow the package-specific architecture and conventions.
- Run focused checks during development, then the applicable package checks before handoff. Record which checks passed, failed, or could not run, and why.

Typical frontend checks, run from `src/dkef-vue`:

```sh
bun install
bun lint
bun run build
```

Use a targeted Vitest file when appropriate. Linting/formatting commands may modify files: review those changes and avoid unrelated churn.

Typical backend checks, run from `src/dkef-api`:

```sh
dotnet restore
dotnet build
```

Run applicable .NET tests if test projects exist in the affected area. The API currently has no test project; use focused local API checks when behavior needs runtime verification. Before `dotnet run`, confirm that connection strings and optional `appsettings.Local.json` point to disposable/local development resources, because startup migrates all six EF contexts.

When a schema change requires a migration, use an explicit context and output directory, for example from `src/dkef-api`:

```sh
dotnet ef migrations add <Name> --context ContentsContext --output-dir Migrations/Contents
```

Do not run the interactive data utility or database updates merely as a build check. Documentation-only changes generally need diff, link, and command-consistency review rather than application execution. Infrastructure changes should be reviewed and statically validated without executing Ansible playbooks or changing shared infrastructure.

## 5. Finish with a pull request and human handoff

Once the acceptance criteria are met and validation is complete:

1. Review `git diff`, run `git diff --check`, and inspect `git status`. Ensure only intended files and no secrets are included.
2. Stage the task files explicitly and create descriptive commits on the feature branch.
3. Push the feature branch, not the default branch.
4. Check for an existing PR for the branch and update it rather than creating a duplicate. Otherwise, open a PR against the discovered default branch using GitHub MCP, with GitHub CLI as the authorization fallback.
5. Look for a repository PR template and use it when present. Describe the goal, changes, validation results, known limitations, and any migration or operational steps requiring human action. Reference the relevant issue when one exists.
6. Report the PR URL and any outstanding risks or blockers to the user. Leave the PR open for human review; do not merge, enable auto-merge, approve your own work, or deploy it.

CLI examples after substituting the actual branch names:

```sh
git push -u origin <feature-branch>
gh pr create --base <default-branch> --head <feature-branch> --title "<summary>" --body-file <pr-description-file>
```

If work is incomplete or checks are blocked, state that clearly; use a draft PR when a partial handoff is appropriate. If authentication or network access prevents pushing or opening a PR, leave the work on the feature branch and provide the exact blocker and human follow-up rather than claiming the contribution is finished. Review feedback should be addressed on the same branch with further validation and an updated PR summary.
