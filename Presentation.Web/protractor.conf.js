var paths = require('../paths.config.js');
var jasmineReporters = require('jasmine-reporters');

exports.config = {
    framework: 'jasmine2',

    // path to the selenium server jar. Update version number accordingly!
    seleniumServerJar: paths.seleniumServerJar,

    // select all end to end tests
    suites: {
        home: 'Tests/home.e2e.spec.js',
        itProject: 'Tests/ItProject/**/*.e2e.spec.js'
    },

    // increase timeout to allow AppVeyor to rebuild database on first instantiation.
    allScriptsTimeout: 30000,
    baseUrl: 'https://localhost:44300',

    onPrepare: function () {
        require('protractor-http-mock').config = {
            rootDirectory: __dirname
        }

        // NUnit xml report
        //jasmine.getEnv().addReporter(new jasmineReporters.NUnitXmlReporter({
        //    reportName: 'Protractor results',
        //    filename: paths.e2eReport + '.xml'
        //}));

        // Terminal output
        jasmine.getEnv().addReporter(new jasmineReporters.TerminalReporter({
            verbosity: 3,
            color: true,
            showStack: false
        }));
        // JUnit xml report
        //jasmine.getEnv().addReporter(new jasmineReporters.JUnitXmlReporter({
        //    filePrefix: paths.e2eReport + '.xml'
        //}));
    },
    // json report
    //resultJsonOutputFile: paths.e2eReport + '.json',

    mocks: {
        default: ['authorize'],
        dir: 'Tests/mocks'
    }
};
