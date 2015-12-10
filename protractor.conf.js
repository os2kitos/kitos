var paths = require('./paths.config.js');
var jasmineReporters = require('jasmine-reporters');

exports.config = {
    framework: 'jasmine2',

    seleniumAddress: 'http://hub.browserstack.com/wd/hub',

    capabilities: {
        'browserstack.user': process.env.BROWSERSTACK_USER,
        'browserstack.key': process.env.BROWSERSTACK_KEY,

        // Needed for testing localhost
        'browserstack.local': 'true',

        // Settings for the browser you want to test
        // (check docs for difference between `browser` and `browserName`
        'browserName': 'Chrome',
        'browser_version': '46.0',
        'os': 'Windows',
        'os_version': '7',
        'resolution': '1280x1024'
    },

    // select all end to end tests
    suites: paths.e2eSuites,

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
    resultJsonOutputFile: paths.e2eReport + '.json',

    mocks: {
        default: ['authorize'],
        dir: paths.source + '/Tests/mocks'
    }
};
