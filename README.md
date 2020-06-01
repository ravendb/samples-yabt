# Yet Another Bug Tracker (YABT)
![.NET Core](https://github.com/ravendb/samples-yabt/workflows/.NET%20Core/badge.svg?branch=master)
<br/>

This is a sample solution to showcase [RavenDB](https://ravendb.net) features with a minimum involvement of third-party dependencies. It also demonstrates some of the best practices like the [Domain Driven Design](https://en.wikipedia.org/wiki/Domain-driven_design) (DDD), [Command Query Responsibility Segregation](https://martinfowler.com/bliki/CQRS.html) (CQRS) and the [Onion Architecture](https://jeffreypalermo.com/2008/07/the-onion-architecture-part-1/).

## Technologies
* ASP .NET Core 3.1
* RavenDB 4.2
* xUnit, NSubstitute

## Getting Started

The easiest way to get started is to check out the solution and run the tests locally:

1. Install the latest [.NET Core SDK](https://dotnet.microsoft.com/download).
2. Open the solution in (VS .NET, VS Code or Rider IDE).
3. Build & Run the tests.

Check out a series of _YABT_ posts at [this blog](https://alex-klaus.com/tags/yabt/) for more information.

## Overview of the solution

### 'Database' project

Contains all entities and aggregates, related classes and enums and DB-related settings.

### 'Domain' project

Contain all interfaces, types and logic specific to the domain layer.

### 'Domain.Tests' project

Automation of test scenarios via Behavior Driven Tests (BDT).

## Support

If you are having problems, please let us know by [raising a new issue](https://github.com/ravendb/samples-yabt/issues/new).

## License

This project is licensed with the [MIT license](LICENSE.md).