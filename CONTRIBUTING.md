# Team Working Rules

## Branches and pull requests
- `main` always builds and passes tests. Nobody commits directly to it.
- Branch names: `feature1/<short-name>`, `feature2/<short-name>`, `feature3/<short-name>`, `web/<short-name>`, `fix/<short-name>`.
- Open a pull request into `main`. At least one teammate reviews before merge. Prefer small pull requests (under about 300 changed lines).
- Before opening a PR: `git pull origin main` into your branch, then `dotnet build` and `dotnet test`.
- Commit messages: one clear sentence in the present tense ("Add circular dependency check").

## Who owns which files
| Area | Files | Owner |
|---|---|---|
| Feature 1 | `ProjectTaskService.cs`, `ProjectsController.cs`, `TasksController.cs`, `ProjectContracts.cs`, `TaskContracts.cs` (Feature 1 parts), `ProjectTaskServiceTests.cs` | Person B |
| Feature 2 | `TaskProgressService.cs`, `TaskProgressController.cs`, `CommentContracts.cs`, `UpdateStatus` parts of `TaskContracts.cs`, `TaskProgressServiceTests.cs` | Person C |
| Feature 3 | `DependencyService.cs`, `TaskDependenciesController.cs`, `DependencyContracts.cs`, `DependencyServiceTests.cs` | Person D |
| Front end | everything in `HorizonLunaris.Web` | Person A |
| Shared | Domain entities, `HorizonLunarisDbContext.cs`, migrations, `Program.cs`, `DtoMapping.cs`, interfaces (`I*.cs`), `Directory.Build.props` | Everyone, under the rules below |

Fill in the real names for Person A to D in `docs/WORKSTREAMS.md`.

## Rules for shared files
1. **`Program.cs`:** only add your own service line. Never reorder or reformat other lines.
2. **`DtoMapping.cs`:** add new methods; do not edit existing ones without telling the team.
3. **Service interfaces and DTOs:** treat them as contracts. Changing a signature breaks someone else's work. Announce it in the team chat first, merge the change by itself in a tiny PR, then tell everyone to pull.
4. **Domain entities and `HorizonLunarisDbContext`:** if you need a schema change, tell the team first. One change at a time.
5. **Migrations:** never edit or delete a committed migration. To change the schema, add a new migration with a descriptive name (`dotnet ef migrations add AddDueDateIndex ...`) in its own small PR. If two people create migrations on different branches, the second person deletes their unmerged migration, pulls main, and re-creates it.
6. **Never commit** `.db` files, `bin/`, `obj/`, or personal settings.

## Definition of done (per feature)
- Every method in the feature's service interface is implemented (no `NotImplementedException` left).
- Business rules from the acceptance criteria are enforced and have unit tests.
- Endpoints work in the `/scalar` page for the right and wrong user (PM vs team member).
- The front end page for the feature works end to end in the browser.
- `dotnet build` and `dotnet test` pass on `main` after merging.

## Communication
- Short daily check-in (chat is fine): what I finished, what I'm doing, what blocks me.
- Announce any change to contracts, entities, or migrations before making it.
- Integration check twice a week: pull `main` and run the demo script in `docs/WORKSTREAMS.md`.
