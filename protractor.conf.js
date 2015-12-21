var paths = require('./paths.config.js');

exports.config = {
    framework: 'jasmine2',

    seleniumAddress: 'http://hub.browserstack.com/wd/hub',

    capabilities: {
        'browserstack.user': process.env.BROWSERSTACK_USER,
        'browserstack.key': process.env.BROWSERSTACK_KEY,

        // needed for testing localhost
        'browserstack.local': 'true',

        // settings for the browser to test
        'browserName': 'Chrome',
        'browser_version': '46.0',
        'os': 'Windows',
        'os_version': '7',
        'resolution': '1280x1024'
    },

    // select all end to end tests
    suites: paths.e2eSuites,

    // increase timeout to allow AppVeyor to rebuild database on first instantiation.
    allScriptsTimeout: 90000,
    baseUrl: 'https://localhost:44300',

    // jasmine timeout options
    jasmineNodeOpts: {
        defaultTimeoutInterval: 90000
    },

    onPrepare: function () {
        require('protractor-http-mock').config = {
            rootDirectory: __dirname
        }

        require("jasmine-expect");
        require("require-dir")("./Presentation.Web/Tests/matchers");
    },
    // json report
    resultJsonOutputFile: paths.e2eReport + '.json',

    mocks: {
        default: ['authorize'],
        dir: paths.source + '/Tests/mocks'
    }
};
