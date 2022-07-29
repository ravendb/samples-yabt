# YABT. Back-end

## Technologies

- ASP .NET Core 6
- RavenDB 5.3

It has minimum third-party dependencies and heaps of best practices like the [Domain Driven Design](https://en.wikipedia.org/wiki/Domain-driven_design) (DDD), [Command Query Responsibility Segregation](https://martinfowler.com/bliki/CQRS.html) (CQRS), etc.

## Overview of the solution

The main projects of the solution:

| Project name    | Description                                                                                                    |
| --------------- | -------------------------------------------------------------------------------------------------------------- |
| Database.Common | Shared auxiliary structures used in DB models and DTOs on the WebAPI.                                          |
| Database        | All entities and aggregates, plus DB-related settings and session. Note: this project is exposed only to `Domain` project. |
| Domain          | Queries and commands, domain layer logic.                                                                      |
| WebAPI          | The application layer (API).                                                                                   |

![Solution diagram](../documentation/projects-diagram.png)

NOTE: all tests are located under ['./tests'](../tests/) folder.

## Getting Started

Firstly, check out the GIT repo.

### Compile and run the tests

1. Install [.NET SDK](https://dotnet.microsoft.com/download) v6.x.<br>
   Note that while the back-end would run on any version of .NET 6, the embedded _RavenDB_ used for the test project has a strong dependency on the _.NET SDK_ version. Check the required version in the [GitHub Action](../.github/workflows/dotnet-core.yml) used to run tests.

2. Open the solution and run the tests located under ['./tests'](../tests/) folder.<br>
   The tests that work against a real embedded _RavenDB_ database and cover multiple scenarios.

### Run Web API (Swagger)

Once you can run tests of the solutions, it's time to spin off a database and run the API.

1. Setup the database:
   1. Acquire a _RavenDB_ instance . Use on of the free options:
      1. free [cloud](https://cloud.ravendb.net/) instance;
      2. [download](https://ravendb.net/download) and install locally for your OS or Docker.
   2. Create a new database on the _RavenDB_ server ([see the docs](https://ravendb.net/docs/article-page/latest/csharp/studio/server/databases/create-new-database/general-flow)).
   3. Import test data from `/documentation/exported_data.ravendbdump` file ([see the docs](https://ravendb.net/docs/article-page/latest/csharp/studio/database/tasks/import-data/import-data-file))
   4. Set the address to the _RavenDB_ server and the DB name in `./back-end/WebApi/appsettings.Development.json`.
2. Launch the solution (the `WebAPI` project).
3. Open `https://localhost:5001/swagger` in the browser.

There is an unsophisticated authentication via a hard-coded API key to run the API end-points. Read a note in the Swagger prompt - it has a clue:
![Swagger screenshot](../documentation/apikey-auth.png)

From here, the back-end is all yours.

Check out [notes on the front-end](../front-end/READM.md) if you'd like to run Angular UI as well.
