# Benraz Microservice Template
This solution represents a template that could be used for easily prepare a new microservice with many features already integrated and ready to use.

## How to use
1. Copy the code from "./src" folder to a destination folder (like {NewProject}/src).
2. Run Rename.ps1 with new project name as a ProjectName parameter (powershell.exe -executionpolicy bypass -file Rename.ps1 -ProjectName "NewProject1").

And that's it, now you have new fully functioning microservice, and it is ready to be extended.

## Features
* Ready to use solution structure;
* Entity Framework Core with database context, mapping configurations, repositories and database migration service;
* Settings for microserviceservice configuration - ability to setup microservice via database;
* Authorization - supports the authorization mechanism using the Benraz Authorization server, verifies the JWT is valid and use the Roles and Claims in the JWT to authorize user access to a microservice resources;
* Data redundancy - checks if the service is running in DR active (otherwise no write access to database), currently it is always returns DR active = true;
* Jobs controller - this controller implements all the microservice tasks as an endpoint (API), those tasks enpoint will be triggered by an enternal scheduler, this mechanism assure this microservice supports Active - Active; the template already implements one 'execute' endpoint (API) as an example.
* Settings controller - a set of API enpoints [GET, POST, PUT, DELETE], that manage Settings (Add setting entry to DB, Get settings from DB, Update settings entry and Delete setting entry);
* IT controller - include an API 'IsAlive' which returns HTTP 200 (OK), this is useful for IT to monitor and verify the microservice is up and running;
* Several appsettings.json files for several environments (QA, Sandbox and Production) - usefull for the DevOps automation, which will be able to use the corresponding appsettings.json of each environment;
* Swagger - a utility to review and use API endpoints via web UI;
* Logging via NLog - logging is configured and integrated into Web API;
* Base EF repository tests and Web API integration tests with NUnit, Fluent Assertions and Moq plugged-in.

## Structure
* \_MicroserviceTemplate\_.Domain. Business entities and services.
* \_MicroserviceTemplate\_.Domain.Tests. Unit tests for business services and business logic.
* \_MicroserviceTemplate\_.EF. Data access layer implementation with Entity Framework Core. Contains:
  * database context;
  * entities configurations;
  * folder for entities data configurations (predefined lookups entries);
  * repositories;
  * database migration service (used for database migration and could be extended with default appendable lookups entries addition).
* \_MicroserviceTemplate\_.EF.Tests. Tests for data access layer, like tests for repositories. Already contains a base class for repository tests.
* \_MicroserviceTemplate\_.WebApi. Web API layer, based on ASP.NET Core Web API technology. Has Swagger, NLog, database context and authorization mechanism plugged in.
* \_MicroserviceTemplate\_.WebApi.IntegrationTests. Integration tests for Web API layer. Already has:
  * Config that prepares server, HTTP client and database context;
  * StartupStub that extends Startup of the server under test and mocks an authorization mechanism to unbind the server under test from Benraz Authorization server;
  * ControllerTestsBase which could be used a base class for all controllers tests.
