# Release management
Kamus releases are somewhat complex, we're aware to that.
Each component has it's own versioning, following semantic versioning rules.
Each component version has it's matching tag on git (e.g. `kamus-cli-0.1`) for tracking version history.
We also maintain "artifical releases" - releases that exists on the release page and on the changelog file. Those releases are used to track closed issues and features - make it easier to understand what change in Kamus.

## Creating a new release
* Create and push a new tag in the format `kamus-<version>` (Due to [this bug][pr-bug], the latest PR is not included in the release - if you need it, make sure to create a dummy commit after it).
* Create new [personal access token](https://github.com/settings/tokens), with `repo` scope.
* Export the token `export GREN_GITHUB_TOKEN=<>`
* Install [gren](https://github.com/github-tools/github-release-notes)
* Create a new release:
```
gren release prerelease
```
* Update changelog file:
```
gren changelog --override 
```

Please note: due to [this issue][issues-bug], gren will fetch only the first 30 issues, if there are more issues - gren will not detect them. There is a partial workaround documented in the issue.

### How releases are created?
Gren will look for all issues closed with PRs that has one of those labels: "enhancement", "documentation" or "bug". If you noticed a missing issue in the release, make sure it has the relevant labels.

[pr-bug]: https://github.com/github-tools/github-release-notes/issues/128
[issues-bug]: https://github.com/github-tools/github-release-notes/issues/209