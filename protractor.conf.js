var paths = require('./paths.config.js');

exports.config = {
    framework: 'jasmine2',

    seleniumAddress: 'http://hub.browserstack.com/wd/hub',

    multiCapabilities: [
        {
            // Chrome 47
            'browserstack.user': process.env.BROWSERSTACK_USER,
            'browserstack.key': process.env.BROWSERSTACK_KEY,
            'browserstack.local': 'true',

            'project': process.env.APPVEYOR_PROJECT_NAME || 'kitos local',
            'build': process.env.APPVEYOR_BUILD_NUMBER || 'local build',

            // settings for the browser to test
            'browserName': 'chrome',
            'browser_version': '47.0',
            'os': 'Windows',
            'os_version': '7',
            'resolution': '1280x1024'
        },
        {
            // IE 10
            'browserstack.user': process.env.BROWSERSTACK_USER,
            'browserstack.key': process.env.BROWSERSTACK_KEY,
            'browserstack.local': 'true',

            'project': process.env.APPVEYOR_PROJECT_NAME || 'kitos local',
            'build': process.env.APPVEYOR_BUILD_NUMBER || 'local build',

            // settings for the browser to test
            'browserName': 'IE',
            'browser_version': '10.0',
            'os': 'Windows',
            'os_version': '7',
            'resolution': '1280x1024'
        },
        {
            // IE 11
            'browserstack.user': process.env.BROWSERSTACK_USER,
            'browserstack.key': process.env.BROWSERSTACK_KEY,
            'browserstack.local': 'true',

            'project': process.env.APPVEYOR_PROJECT_NAME || 'kitos local',
            'build': process.env.APPVEYOR_BUILD_NUMBER || 'local build',

            // settings for the browser to test
            'browserName': 'IE',
            'browser_version': '11.0',
            'os': 'Windows',
            'os_version': '7',
            'resolution': '1280x1024'
        },
        {
            // Edge 12
            'browserstack.user': process.env.BROWSERSTACK_USER,
            'browserstack.key': process.env.BROWSERSTACK_KEY,
            'browserstack.local': 'true',

            'project': process.env.APPVEYOR_PROJECT_NAME || 'kitos local',
            'build': process.env.APPVEYOR_BUILD_NUMBER || 'local build',

            // settings for the browser to test
            'browserName': 'Edge',
            'browser_version': '12.0',
            'os': 'Windows',
            'os_version': '10',
            'resolution': '1280x1024'
        },
        {
            // Firefox 42
            'browserstack.user': process.env.BROWSERSTACK_USER,
            'browserstack.key': process.env.BROWSERSTACK_KEY,
            'browserstack.local': 'true',

            'project': process.env.APPVEYOR_PROJECT_NAME || 'kitos local',
            'build': process.env.APPVEYOR_BUILD_NUMBER || 'local build',

            // settings for the browser to test
            'browserName': 'Firefox',
            'browser_version': '42.0',
            'os': 'Windows',
            'os_version': '7',
            'resolution': '1280x1024'
        }
    ],

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
        };

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
