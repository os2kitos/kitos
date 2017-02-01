# Technical documentation of OS2KITOS 2.0

Here follows a short documentation of the development setup for OS2KITOS 2.0.

OS2KITOS consists of two major parts. The backend WebAPI and OData API written in C# and .NET and an [AngularJS 1.4.7](https://angularjs.org/) frontend written in [TypeScript](http://www.typescriptlang.org/).

# Frontend

Dependencies for the frontend application is managed with the [Node.js](https://nodejs.org/en/) package manager `npm` and the node module [Bower](http://bower.io/). Bundling, linting and testing is handled with [Gulp](http://gulpjs.com/).
Before using the application install Node.js 4.x or above.

## Install dependencies
```
npm install -g bower gulp typings
```
This installs `bower`, `gulp` and `typings` globally. This is not nessesary but assumed in the rest of the documentation.
```
npm install
```
Install Node.js modules. (Bower dependencies are installed through a postinstall script)
```
typings install
```
Install typings used by TypeScript

## Deployment
There are `gulp` tasks to handle minification and concatination of all compiled TypeScript files and library dependencies. The main task is `gulp deploy`. This task is automatically runs on build when using Visual Studio 2015 by means of the Task Runner Explorer.

What files to bundle is managed in `bundle.config.js`.

# Backend
The backend is a mix of .NET WebApi and OData controllers. Tests are written with [xUnit](http://xunit.github.io/) with the [NSubstitute](http://nsubstitute.github.io/) substition framework.

Packages are managed with NuGET.

## Unit testing

There are several `.bat` scripts to run backend tests. 

 * `run-coverage.bat` runs all tests and creates an XML coverage report
 * `run-local-coverage.bat` runs all tests and create an HTML coverage report which is opened after tests
 * `run-tests.bat` runs the xUnit tests. This is referenced in the other scripts
 
Note that these files must be updated with the right paths when there are version changes to [OpenCover](https://github.com/OpenCover/opencover) and [ReportGenerator](https://github.com/danielpalme/ReportGenerator).

There is only a small amount of unit tests for the excisting backend. The goal is to rewrite all endpoints to OData controllers. The considerations of this change are not yet done, why no tests are written.

# CI
TeamCity