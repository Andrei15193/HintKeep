## HintKeep
A small application for managing password hints.

## Technical Goals
The aim of the application is to make a practical and useful application that applies a number of architectural patterns as close as they can be to their definition in order to both exercise them and understand them better.

* Apply the REST architectural pattern as much as it can be possible, appropriate routes, HTTP response codes etc. The transfer content is done completely through JSON objects
* Apply the CQRS architectural pattern as much as it can be possible on the back-end side
* Apply the MVVM architectural pattern as much as it can be possible on the front-end side to demonstrate an alternative to the Flux architectural pattern
* Use Azure Table Storage for a database, can be optimized for querying over editing as in most cases the data will be read rather than modified
* Use translation keys for everythign that comes from the API, translation happens on the front-end exclusively (except for user entered values)