[![Build Status](https://dev.azure.com/umbraco/Umbraco%20Headless/_apis/build/status/umbraco.join-monster-dotnet?branchName=master)](https://dev.azure.com/umbraco/Umbraco%20Headless/_build/latest?definitionId=263&branchName=master)
[![NuGet version (JoinMonster)](https://img.shields.io/nuget/v/JoinMonster.svg?style=flat-square)](https://www.nuget.org/packages/JoinMonster/)

# Join Monster for .NET

A .Net implementation of the [Join Monster](https://github.com/join-monster/join-monster) NodeJS library.

## Introduction

Join Monster for .NET is a query planner and batch data fetcher between GraphQL and SQL built for [GraphQL for NET](https://github.com/graphql-dotnet/graphql-dotnet).

**NOTE**: While the project is considered stable, the public API will have breaking changes with each release before v1.0.

For now the supported databases are PostgreSQL and SQLite (limited support).

## Usage

Take a look at the [Star Wars Sample](../samples/StarWars).

## TODO

-   [x] Where conditions
-   [x] Order by
-   [x] One-To-Many joins
-   [x] Many-To-Many joins
-   [x] Paging
-   [x] Connections
-   [ ] Better SQL query abstraction
-   [ ] Query batching
-   [ ] Support for more databases
-   [ ] Documentation
