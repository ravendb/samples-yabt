# YABT. Tests
- `Database.Tests` – tests for the indexes and entities (to a lesser extent).<br>
  That's mostly used in TDD.
- `Database.Migration.Tests` – tests for the migration logic.<br>
  Reduces risks of messing up the production database on deploying critical DB update.
- `Domain.Tests` – tests for the domain services.<br>
Integration tests to ensure the expected behaviour around querying/filtering data and persisting changes. Applies Behavior Driven Tests (BDT).  

## Technology
* xUnit, NSubstitute
* RavenDB.TestDriver
* .NET 6

The tests that work against a real embedded _RavenDB_ database.

See more on writing RavenDB tests in the [official docs](https://ravendb.net/docs/article-page/latests/csharp/start/test-driver).