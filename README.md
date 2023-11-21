# Events Logger

## introduction

<!--This should give a brief story of the application. -->

Events Logger is web application made by students of [UNIFEI](https://unifei.edu.br) in an extension program that has the purpose of giving participants a hands-on approach in a real-world scenario.

This project tries to be a way to log and display events associated with tasks
construction workers perform daily, it gives the users the ability to create a construction project and manage the workers and managers logs.

## Features

<!--Features: This should highlight the application’s key features.-->

- Users can signup and login to their accounts
- Authenticated users can make CRUD operations on their entries, projects and manage everything on his project.
  - the owner of a project can add, remove, update and see a worker that's part of his project
  - the owner of a project can add, remove, update and see all the entries of the project
  - every user can upload files to the entries and have a profile photo

## Installation guide

<!--
    Installation guide: This should guide our user get the development environment up and running.
 -->

- Clone this repository [here](https://github.com/LordSouza/Events-Logger-Backend.git).
- The main branch is the most stable branch at any given time, ensure you're working from it.
- You need to update the necessary connections strings [here](EventsLogger.Api\appsettings.json)
  - ConnectionStrings:DefaultConnection -> connection string for PostgreSQL
  - StorageConfig:BlobConnection -> blob storage connection string, this project used [azure blob storage](https://azure.microsoft.com/en-us/products/storage/blobs)
  - JwtConfig:Secret -> set the salt of the encription for the login token

## How to use

<!--
    Usage: This should explain how the application works.
 -->

- Go the folder EventsLogger.Api and run `dotnet run`
- You can use swagger documentation on development enviroment or postman
- The [Postmand import file](EventsLogger.postman_collection.json) also has all the endpoints working

## API endpoints

<!--
    API endpoints: This will be a list of all created endpoints and expected responses.
 -->

### Auth

| HTTP Verbs | Endpoints          | Action                            |
| ---------- | ------------------ | --------------------------------- |
| POST       | /api/Auth/Register | To sign up a new user account     |
| POST       | /api/Auth/Login    | To login an existing user account |

### User

| HTTP Verbs | Endpoints     | Action                                     |
| ---------- | ------------- | ------------------------------------------ |
| POST       | /api/User/All | To retrieve all users on the plataform     |
| DELETE     | /api/User     | To delete the logged account               |
| PUT        | /api/User     | To edit the details of the logged user     |
| GET        | /{username}   | To retrieve a single user with his entries |

### Entry

| HTTP Verbs | Endpoints       | Action                                                                                               |
| ---------- | --------------- | ---------------------------------------------------------------------------------------------------- |
| GET        | /api/Entry      | To retrieve all entries of all projects the user is part of, this end point have filter capabilities |
| POST       | /api/Entry      | To create a new entry for the logged user                                                            |
| GET        | /api/Entry/{id} | To retrieve a single entry with the project and user associate with                                  |
| DELETE     | /api/Entry/{id} | To delete a entry of the logged user                                                                 |
| PUT        | /api/Entry/{id} | To update a entry of the logged user                                                                 |

### Project

| HTTP Verbs | Endpoints         | Action                                                                 |
| ---------- | ----------------- | ---------------------------------------------------------------------- |
| GET        | /api/Project      | To retrieve all project the user is part of                            |
| POST       | /api/Project      | To create a new project for the logged user                            |
| GET        | /api/Project/{id} | To retrieve a single Project with the entries and users associate with |
| DELETE     | /api/Project/{id} | To delete a project of the logged user                                 |
| PUT        | /api/Project/{id} | To update a project of the logged user                                 |

### ProjectEntries

| HTTP Verbs | Endpoints               | Action                                                                         |
| ---------- | ----------------------- | ------------------------------------------------------------------------------ |
| POST       | /api/Project/Entry      | To create a new entry for a user inside the project the logged user is part of |
| DELETE     | /api/Project/Entry/{id} | To delete a entry for a user inside the project the logged user is part of     |
| PUT        | /api/Project/Entry/{id} | To update a new entry for a user inside the project the logged user is part of |

### ProjectUsers

| HTTP Verbs | Endpoints                | Action                                                                   |
| ---------- | ------------------------ | ------------------------------------------------------------------------ |
| PUT        | /api/Project/User        | To update a user inside the project the logged user is part of           |
| POST       | /api/Project/User        | To add a user inside the project the logged user is part of              |
| DELETE     | /api/Project/User        | To remove a user of the project the logged user is part of               |
| POST       | /api/Project/User/Create | To create a shadow user inside the project the logged user is part of    |
| DELETE     | /api/Project/User/Create | To delete a user which is part of the project the logged user is part of |

## Commit n branch standard

tree based like method

### branch:

commit-type/commit_description

### commit:

[**commit_type**] - "commit description"

commit-type:

- FEAT - New feature
- FIX - Correction
- IMP - Improvement of something
- REM - removing things
- WIP - Work in progress

## Technologies used

<!--
    Technologies used: This will list all the technologies the application is built with.
 -->

- [C#](https://dotnet.microsoft.com/pt-br/languages/csharp) This is a modern, innovative, open-source, cross-platform object-oriented programming language

- [.NET](https://dotnet.microsoft.com/en-us/) This is a free, open-source, cross-platform framework for building modern apps and powerful cloud services.

- [PostgreSQL](https://www.postgresql.org/) This is a powerful, open source object-relational database system.

<!-- TODO packages used -->

- [AutoMapper](https://www.nuget.org/packages/AutoMapper) This nuget package make it easier to map entities to data transfers objects.

-[Azure.Storage.Blobs](https://www.nuget.org/packages/Azure.Storage.Blobs) This is Microsoft's object storage solution for the cloud. Blob storage is optimized for storing massive amounts of unstructured data. Unstructured data is data that does not adhere to a particular data model or definition, such as text or binary data.

- [Microsoft.AspNetCore.Authentication.JwtBearer](https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.JwtBearer) ASP.NET Core middleware that enables an application to receive an OpenID Connect bearer token.

- [Microsoft.AspNetCore.Identity.EntityFrameworkCore](https://www.nuget.org/packages/Microsoft.AspNetCore.Identity.EntityFrameworkCore) ASP.NET Core Identity provider that uses Entity Framework Core.

- [ Npgsql.EntityFrameworkCore.PostgreSQL](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL) This is the open source EF Core provider for PostgreSQL. It allows to interact with PostgreSQL via the most widely-used .NET O/RM from Microsoft, and use familiar LINQ syntax to express queries.

## Authors

<!--
    Authors: A list of authors and contributors to this project.
 -->

- [Lucas Oliveira](https://github.com/LordSouza)
- [João Prado](https://github.com/WinterDP)

## License

This project is available for use under the MIT License.

## Acknowledgements

I want to thank the [Cultural Homestay International](https://www.linkedin.com/company/cultural-homestay-international/) for the amazing oportunity, and making available some time of their lead engineers to give us feedback and insights on our progress.
