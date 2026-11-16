---
name: upgrade-nuget-packages
description: Upgrade all NuGet package versions in this repository to the latest stable releases, validate the build and integration tests, and open a pull request.
---

# Upgrade NuGet packages

Use this skill when asked to upgrade this repository's NuGet dependencies. Work from the repository root. Do not publish packages or merge the pull request.

## Prepare

1. Check `git status --porcelain` and the current branch. If the worktree has any staged, unstaged, or untracked changes, stop and ask the user to provide a clean worktree; do not stash or overwrite their work.
2. Identify the repository's default branch and remote with `gh repo view --json defaultBranchRef,nameWithOwner` and `git remote -v`. Fetch the default branch and create a uniquely named `chore/upgrade-nuget-<date>` branch from its up-to-date remote tracking ref. Never reset or reuse an existing branch.
3. Inspect `Directory.Packages.props`, the project files, any NuGet configuration or lock files, the solution, CI workflow, and any pull request template. Versions are currently centrally managed in `Directory.Packages.props`; the solution is `DristributedLockManager.sln` (note the spelling).
4. Ensure the installed .NET SDK supports the projects' target frameworks (currently .NET 10). Integration tests require a working Docker daemon and a pullable `redis:latest` image. Check `docker info`; if Docker is unavailable, stop and report the blocker rather than skipping tests or starting a shared daemon without permission.

## Upgrade

1. Enumerate all explicitly declared NuGet package versions, including centrally managed entries, project-level overrides, and any package lock files. Use the repository's configured package sources; do not ignore failed feeds. Upgrade to the latest **stable** version of each package, including major versions. Only include prereleases if explicitly requested. Do not promote transitive dependencies to direct references just to upgrade them.
2. Use `dotnet-outdated-tool` to discover and apply updates across the solution. If it is not already available, create an isolated temporary directory outside the repository and install it with `dotnet tool install --tool-path <temporary-directory> dotnet-outdated-tool`. Run the tool executable with:

   ```bash
   dotnet-outdated DristributedLockManager.sln --pre-release Never --version-lock None --upgrade
   ```

   When using an isolated tool directory, invoke its `dotnet-outdated` executable by path. If prereleases were explicitly requested, use `--pre-release Always` instead. Do not add a tool manifest or global installation to the repository.
3. Review the tool output and diff. Independently check every explicit package declaration against the configured sources, including entries not referenced by a solution project, and update any missed versions. Do not assume the tool upgraded everything: it can retain versions because of compatibility or version ranges. If a latest version cannot be determined or used, report the package and reason and do not open a PR claiming a complete upgrade.
4. Make only changes needed for the upgrade, including minimal API or test changes required by newer packages. Preserve central version management. Update lock files if present. If CI's SDK cannot build the current target framework, update the CI SDK as needed so the upgrade PR can be validated; currently `.github/workflows/ci.yml` specifies .NET 9 while the projects target .NET 10.
5. Recheck for outdated explicit packages and inspect the diff. If there are no package upgrades, stop and report that everything is current; do not create a commit or PR.

## Validate and open the PR

1. Run the following against the solution, stopping on any failure:

   ```bash
   dotnet restore DristributedLockManager.sln
   dotnet build DristributedLockManager.sln --configuration Release --no-restore
   dotnet test DristributedLockManager.sln --configuration Release --no-build --verbosity normal
   ```

   Confirm that the integration tests actually ran and passed; zero tests, skipped tests, Docker failures, restore errors, or build failures are not success. Resolve upgrade-related failures and rerun validation. If unable to validate all changes or upgrade every package, report the blocker and do not create a PR.
2. Review `git diff` and `git status`, stage only the intended files, and commit with a descriptive message and the `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>` trailer. Push the new branch to the repository remote.
3. After the push succeeds, create a PR targeting the default branch, following the repository's PR template if present. Summarize the package upgrades, any compatibility or CI changes, and the actual validation results. Use the session's PR-creation tool if available, otherwise `gh pr create`. Do not create a PR if push or validation fails.
4. Remove only temporary tool files created for this task, including when stopping on a blocker, and report the PR or the specific blocker.
