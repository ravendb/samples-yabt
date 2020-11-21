# Yet Another Bug Tracker (YABT)
![.NET Core](https://github.com/ravendb/samples-yabt/workflows/.NET%20Core/badge.svg?branch=master)
<br/>

A sample solution to showcase [RavenDB](https://ravendb.net) features accompanied with a [series of articles](https://ravendb.net/articles/building-application-with-net-core-and-ravendb-nosql-database). It has minimum third-party dependencies and many examples of the best practices like the [Domain Driven Design](https://en.wikipedia.org/wiki/Domain-driven_design) (DDD), [Command Query Responsibility Segregation](https://martinfowler.com/bliki/CQRS.html) (CQRS) and the [Onion Architecture](https://jeffreypalermo.com/2008/07/the-onion-architecture-part-1/).

## Technologies
* ASP .NET Core 5.0
* RavenDB 5.1
* xUnit, NSubstitute

## Getting Started

The easiest way to get started is to check out the solution and run the tests locally (WebAPI and UI will be added later):

1. Install the latest [.NET Core SDK](https://dotnet.microsoft.com/download).
2. Open the solution (e.g. in VS .NET, VS Code or Rider IDE).
3. Build & fiddle with the tests.<br>Want more?
4. Acquire a RavenDB instance (either in the [cloud](https://cloud.ravendb.net/) or [download](https://ravendb.net/download) and install locally).
5. Run the WebAPI (Swagger).

Check out a series of _YABT_ articles on the [RavenDB website](https://ravendb.net/articles/building-application-with-net-core-and-ravendb-nosql-database).

## Overview of the solution

|Project name|Description|
| ---------- | ----------|
|Database|All entities and aggregates, related classes and enums and DB-related settings.|
|Domain|All interfaces, types and logic specific to the domain layer.|
|WebAPI|The application layer (WebAPI).|
|Domain.Tests|Automation of test scenarios applying Behavior Driven Tests (BDT).|

## Support

If you are having problems, please let us know by [raising a new issue](https://github.com/ravendb/samples-yabt/issues/new).

## License

This project is licensed with the [MIT license](LICENSE).
