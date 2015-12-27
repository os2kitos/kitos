var paths = require('./paths.config.js');

exports.config = {
    framework: 'jasmine2',

    seleniumAddress: 'http://localhost:4444/wd/hub',

    capabilities: {
        browserName: 'chrome'
    },

    // select all end to end tests
    suites: paths.e2eSuites,

    // increase timeout to allow AppVeyor to rebuild database on first instantiation.
    allScriptsTimeout: 30000,
    baseUrl: 'https://localhost:44300',

    onPrepare: function () {
        require('protractor-http-mock').config = {
            rootDirectory: __dirname
        };

        require("jasmine-expect");
        require("require-dir")("./Presentation.Web/Tests/matchers");

        var reporters = require("jasmine-reporters");
        jasmine.getEnv().addReporter(new reporters.TerminalReporter({
            verbosity: 3,
            color: true,
            showStack: true
        }));
    },

    jasmineNodeOpts: {
        print: function () { }
    },

    mocks: {
        default: ['authorize'],
        dir: paths.source + '/Tests/mocks'
    }
};
