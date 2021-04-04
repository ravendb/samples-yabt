# Yet Another Bug Tracker (YABT)
[![.NET Build & Tests](https://github.com/ravendb/samples-yabt/actions/workflows/dotnet-core.yml/badge.svg)](https://github.com/ravendb/samples-yabt/actions/workflows/dotnet-core.yml)
[![Angular front-end](https://github.com/ravendb/samples-yabt/actions/workflows/angular.yml/badge.svg)](https://github.com/ravendb/samples-yabt/actions/workflows/angular.yml)
<br/>

A sample solution to showcase [RavenDB](https://ravendb.net) features accompanied with a [series of articles](https://ravendb.net/news/use-cases/yabt-series). Available live at [yabt.ravendb.net](https://yabt.ravendb.net) (and API at [yabt.ravendb.net/swagger](https://yabt.ravendb.net/swagger/index.html)). 

It has minimum third-party dependencies and heaps of best practices like the [Domain Driven Design](https://en.wikipedia.org/wiki/Domain-driven_design) (DDD), [Command Query Responsibility Segregation](https://martinfowler.com/bliki/CQRS.html) (CQRS), [Onion Architecture](https://jeffreypalermo.com/2008/07/the-onion-architecture-part-1/), etc.

## Technologies
* ASP .NET Core 5.0
* RavenDB 5.1
* Angular 11

## Getting Started

You can poke around the live instance via the front-end at [yabt.ravendb.net](https://yabt.ravendb.net) or API at [yabt.ravendb.net/swagger](https://yabt.ravendb.net/swagger/index.html).

### Get it up locally

Firstly
1. Check out the GIT repo.
2. Install the latest [.NET Core SDK](https://dotnet.microsoft.com/download).
3. Open the solution (e.g. in VS .NET, VS Code or Rider IDE).

Now you can build the solution and fiddle with the tests that work against a real embedded RavenDB database and cover multiple scenarios.

[See description](back-end/README.md) in the `back-end` folder to learn more about the back-end implementation.

#### Next - Run API

1. Acquire a RavenDB instance (either a free instance in the [cloud](https://cloud.ravendb.net/) or [download](https://ravendb.net/download) and install locally).
2. Set the address to the DB in `appsettings.json`.
3. Run the WebAPI (Swagger).

#### Last - Run the front-end

1. Install the latest LTS versions of [Node.js](https://nodejs.org).
2. Open `front-end` folder and initialize the project by running `npm i` command.
3. Build and run by `npm start`.

## Support

If you are having problems, please let us know by [raising a new issue](https://github.com/ravendb/samples-yabt/issues/new) or contacting the author on Twitter @ [_AlexKlaus](https://twitter.com/_AlexKlaus).

## License

This project is licensed with the [MIT license](LICENSE).
