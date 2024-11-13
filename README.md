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

## Database

if *ConnectionStrings* variable null, empty or not exist, the service will not initiate the EF, migration and if any call require access to DB, exception will be thrown, but if call other stuff such as IsAlive() endpoint, the service will be loaded and the endpoint call will response correctly.

## Templates

1. src-msdb : a project with entity framwark, setting table for configuration, .env for secrets (devops pipeline) and appsetting.json
2. scr-env (msdb): env for all configurations, no appsetting.json, DB for job, and one table as example (remove setting table and controller, and add myTable with the controller (CURD)
3. scr-env (postgressdb):  -TBD

## Environment Variables

At first we are using the appsetting.json, while also some of the setting are located in the setting table in DB.
we can't add secrets to appsetting.json thus, those secret was in that table.

The best practices to save secrets are to save them in Key vault, additionally managing settings in database is a bit awakeward, thus we added the following mechanisem:

By default the service will use the environment variables, means, if there is no **appsetting.json** file or UseEnvironmentVariables variable in the **appsetting.json** file.

When **appsetting.json** file includes *UseEnvironmentVariables=false* then the service will use the appsetting.json as the source of the settings variables.

All environment variables should be in camelCase.

### Secrets

1. We have added .env  folder which includes the environment variables (.env file) for each region (environment-region: dev-use1 = usa est1)
2. Secrets variables include values with "__" in the beginning and end of the Key name in the keyVault, example DB_PASS=\_\_KEY_DB_PASS\_\_.
3. Pipeline will handle these secrets, pipeline before inject the value of DB_PASS in the container environment secrets, it first will pull the secret value from keyVault, the key name in this case is KEY_DB_PASS.
