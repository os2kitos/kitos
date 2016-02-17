# Technical documentation of KITOS

Here follows a short documentation of the development setup for KITOS.

KITOS consists of two major parts. The backend WebAPI and OData API written in C# and .NET and an AngularJS frontend written in Typescript.

## Frontend

Dependencies for the frontend application is managed with node.js packages manager `npm` and the node module `bower`.
Before using the application install Node.js 4.x or above.

### Install dependencies
Install node.js modules and bower dependencies with

```
npm install -g bower gulp
npm install
bower install
```

This install bower and gulp globally. The latter one for frontend testing.

### Testing

The frontend is integration tested with `protractor` and unit tested with `karma`. Tests are written in `jasmine` with some additional expection libraries.

#### Unit testing
Run unit tests locally with
```
gulp localCover
```
This runs the unit tests in a headless browser and displays coverage when the tests are done. See the `gulp/test.js` for details on the available `gulp` tasks.

#### Integration testing
Run integration tests locally with

```
start gulp webdriver
gulp locale2e
```

The first command opens a new `cmd` window. Minimize this and return to the first window to run the tests.
To specify what suite to test add the attributes `--suite name` like: `gulp locale2e --suite mySuite`.

On first run install the standalone webdriver before the above commands with
```
node_modules\.bin\webdriver-manager update --standalone
```

Suites are specified in `paths.config.js`.

Protractor configuration is split in a local and CI file: `protractor.local.conf.js` and `protractor.conf.js`. The former one only runs tests in Chrome. The latter one runs tests on the Browserstack webservice.

## Backend
The backend is a mix of .NET WebApi controllers and OData controllers. Tests are written with xUnit with the NUnit substition framework.

Packages are managed with NuGET.

### Unit testing

There are several `.bat` scripts to run backend tests. 

 * `run-coverage.bat` runs all tests and creates an XML coverage report
 * `run-local-coverage.bat` runs all tests and create an HTML coverage report which is opened after tests
 * `run-tests.bat` runs the xUnit tests. This is referenced in the other scripts
 
Note that these files must be updated with the right paths when there are version changes to OpenCover and ReportGenerator.

## CI
The project is setup with a Continious Integration Server at AppVeyor
