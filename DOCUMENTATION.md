# Technical documentation of KITOS

Here follows a short documentation of the development setup for KITOS.

KITOS consists of two major parts. The backend WebAPI and OData API written in C# and .NET and an [AngularJS 1.x](https://angularjs.org/) frontend written in [Typescript](http://www.typescriptlang.org/).

# Frontend

Dependencies for the frontend application is managed with the [Node.js](https://nodejs.org/en/) package manager `npm` and the node module [Bower](http://bower.io/). Bundling, linting and testing is handled with [Gulp](http://gulpjs.com/).
Before using the application install Node.js 4.x or above.

## Install dependencies
Install Node.js modules and bower dependencies with

```
npm install -g bower gulp
npm install
bower install
```

This installs `bower` and `gulp` globally. This is not nessesary but assumed in the rest of the documentation.

## Testing

The frontend is integration tested with [Protractor](https://angular.github.io/protractor/#/) and unit tested with [Karma](https://karma-runner.github.io/). Tests are written in [Jasmine](http://jasmine.github.io/) with some additional expection libraries.

### Unit testing
Run unit tests locally with

    gulp localCover

Tests are run in a headless browser and displays coverage when the tests are done in a browser. See `gulp/test.js` for details on the available `gulp` tasks. Local testing does not create source maps for the typescript files. This is only done when running the task `gulp codecov`.

### Integration testing
Run integration tests locally with

   start gulp webdriver
   gulp locale2e

The first command opens a new `cmd` window. Minimize this and return to the first window to run the tests. This creates the selenium tunnel that Protractor needs.

To specify what suite to test add the attributes `--suite name` like: `gulp locale2e --suite mySuite`.

**Note on first install**: The standalone webdriver must be installed before the above commands can run:

    node_modules\.bin\webdriver-manager update --standalone

Suites are specified in `paths.config.js`.

Protractor configuration is split in a local and CI configuration file: `protractor.local.conf.js` and `protractor.conf.js`. The former one only runs tests in Chrome. The latter one runs tests on the [Browserstack](https://www.browserstack.com/) webservice.

## Deployment
There are `gulp` tasks to handle minification and concatination of all compiled typescript files and library dependencies. The main task is `gulp deploy`. This task is automatically runs on build when using Visual Studio 2015 by means of the Task Runner Explorer.

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
The project is setup with a Continious Integration Server at [AppVeyor](http://www.appveyor.com/). The server is triggered on commits to all branches and on pull requests. When the build is done it reports coverage to the [codecov.io](https://codecov.io/) webservice.

Configuration of AppVeyor is done with `appveyor.yml`. Do note that it is possible to configure settings on the AppVeyor webservice, but one should only use the `appveyor.yml` to persist changes.
