## HintKeep
A small application for managing password hints.

## Technical Goals
The aim of the application is to make a practical and useful application that applies a number of architectural patterns as close as they can be to their definition in order to both exercise them and understand them better.

* Apply the REST architectural pattern as much as it can be possible, appropriate routes, HTTP response codes etc. The transfer content is done completely through JSON objects
* Apply the CQRS architectural pattern as much as it can be possible on the back-end side
* Apply the MVVM architectural pattern as much as it can be possible on the front-end side to demonstrate an alternative to the Flux architectural pattern
* Use Azure Table Storage for a database, can be optimized for querying over editing as in most cases the data will be read rather than modified
* Use translation keys for everythign that comes from the API, translation happens on the front-end exclusively (except for user entered values)

## Project Patterns
The `Controllers` directory contains the controllers that handle requests. Each controller corresponds to an area of an application (e.g.: users, hints).

The project uses CQRS to handle requests, The `Requests` and `RequestsHandlers` each contain scoped definitions for requests (commands and querries) and their handlers respectively. The scoping is done by area of the application, for instance, the commands and queries related to `Users` are contained in a directory with the same name.

The communication between requests (both HTTP and query results) and controllers is done through view models, all of them are defined in the `ViewModels` direcotry where they are scoped by the area they cover (e.g.: `Users` contains view models that represent users). Note that view models do not map directly to the commands and queries scopes, a command or query relating to password hints may use a user view model to represent the owner of said password hint. Along side each scope, the `ViewModels` directory contains a `ValidationAttributes` directory which contains all attributes used to validate view model properties. The ones provided with .NET are not directly provided as the error messages for each validation is a translation label rather than a translated text. Translation is handled on the front-end side.

The `Services` directory contains all service interface definitions (along side the data transfer objects that are used by these services as they are considered to be part of the interface), while the `Implementations` subdirectory contains their implementations. Each implementation that requires configuration has an associated configuration object that reads data from an [IConfigurationSection](https://docs.microsoft.com/dotnet/api/microsoft.extensions.configuration.iconfigurationsection) which is defined in the `Configs` subdirectory of the `Implementations` one. The naming pattern contains the base service name, the interface starts with an `I` the config one has a `Config` at the end.

```
ICustomService        <- service interface (under Services)
 CustomService        <- service implementation (under Services/Implementations)
 CustomServiceConfig  <- config definition (under Services/Implementations/Configs)
```

Each service is configured in a section having the same name as the service base name.

```json
{
  "SaltService": {
    "SaltLength": 10
  },
  "CryptographicHashService": {
    "HashAlgorithm": "SHA512"
  }
}
```

The `Storage` directory contains all definitions for handling storage of data. The `Entities` contains table entity definitions which are stored in Azure Table Storage. The `IEntityTables` interface exposes all defined tables in the application.

## Tests Patterns
The tests project (HintKeep.Tests) contains both unit tests and integration tests. They are split at the root directory into `Integration` and `Unit` which contains their respective tests.

Integration tests are testing the application components from the API level, they map to endpoints and use in-memory data store in order to offer isolation between tests as well as improving speed. Each endpoint is tested for expected HTTP status codes for both happy flows and less happy flows (data validation), the point is to ensure not only that the components integrate but also provide a means to allow easy refactoring as these tests will cover a lot of the cases which would make one comfortable to relly on them to know something went wrong.

Unit tests are the most common tests, everyone should know about them and have quite a lot of them written to cover as many scenarios as possible. The directory/namespace structure maps the one in the application project.

_To be determined_, system testing should be implemented as well, but these should go through application flows (e.g.: user registration, adding a password hint etc.). But it is not yet decided at which level they will be implemented, e.g.: API level, go through multiple APIs as if the user was navigating through the application and it would call these APIs, or at the UI level, actually simulate user interaction. The former is easier to implement as some services/objects can be mocked (data store and e-mail integrations would be the prime candidates for this) while the latter covers more of the application as the UI is included as well, but it is harder to mock services/objects, if not impossible as the API server needs to be running for the UI to work.

Along side `Unit` and `Integration` there is a 3rd directory, `Stubs`, which contains all in-memory implementations of different interfaces. These are created to replace external dependencies such as databases and email service. This will both ensure isolation from external dependencies and between tests while allowing them to run more quickly and in parallel. The complete integration with external systems is done through system testing.