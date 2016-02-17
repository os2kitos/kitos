var paths = require('./paths.config.js');

exports.config = {
    framework: 'jasmine2',

    browserstackUser: process.env.BROWSERSTACK_USER,
    browserstackKey: process.env.BROWSERSTACK_KEY,

    multiCapabilities: [
        {
            // Chrome 47
            'browserstack.local': 'true',

            'project': process.env.APPVEYOR_PROJECT_NAME || 'kitos local',
            'build': process.env.APPVEYOR_BUILD_NUMBER || 'local build',

            // settings for the browser to test
            'browserName': 'chrome',
            'browser_version': '47.0',
            'os': 'Windows',
            'os_version': '7',
            'resolution': '1280x1024',

            'acceptSslCerts': 'true'
        }
        ,
        //{
        //    // IE 10
        //    'browserstack.local': 'true',

        //    'project': process.env.APPVEYOR_PROJECT_NAME || 'kitos local',
        //    'build': process.env.APPVEYOR_BUILD_NUMBER || 'local build',

        //    // settings for the browser to test
        //    'browserName': 'IE',
        //    'browser_version': '10.0',
        //    'os': 'Windows',
        //    'os_version': '7',
        //    'resolution': '1280x1024',

        //    'acceptSslCerts': 'true'
        //},
        //{
        //    // IE 11
        //    'browserstack.local': 'true',

        //    'project': process.env.APPVEYOR_PROJECT_NAME || 'kitos local',
        //    'build': process.env.APPVEYOR_BUILD_NUMBER || 'local build',

        //    // settings for the browser to test
        //    'browserName': 'IE',
        //    'browser_version': '11.0',
        //    'os': 'Windows',
        //    'os_version': '7',
        //    'resolution': '1280x1024',

        //    'acceptSslCerts': 'true'
        //},
        //{
        //    // Edge 12
        //    'browserstack.local': 'true',

        //    'project': process.env.APPVEYOR_PROJECT_NAME || 'kitos local',
        //    'build': process.env.APPVEYOR_BUILD_NUMBER || 'local build',

        //    // settings for the browser to test
        //    'browserName': 'Edge',
        //    'browser_version': '12.0',
        //    'os': 'Windows',
        //    'os_version': '10',
        //    'resolution': '1280x1024',

        //    'acceptSslCerts': 'true'
        //},
        //{
        //    // Firefox 42
        //    'browserstack.local': 'true',

        //    'project': process.env.APPVEYOR_PROJECT_NAME || 'kitos local',
        //    'build': process.env.APPVEYOR_BUILD_NUMBER || 'local build',

        //    // settings for the browser to test
        //    'browserName': 'Firefox',
        //    'browser_version': '42.0',
        //    'os': 'Windows',
        //    'os_version': '7',
        //    'resolution': '1280x1024',

        //    'acceptSslCerts': 'true'
        //}
    ],

    // select all end to end tests
    suites: paths.e2eSuites,

    // increase timeout to allow AppVeyor to rebuild database on first instantiation.
    allScriptsTimeout: 90000,
    baseUrl: 'https://localhost:44300',

    jasmineNodeOpts: {
        defaultTimeoutInterval: 90000,
        print: function () { }
    },

    onPrepare: function () {
        require('protractor-http-mock').config = {
            rootDirectory: __dirname
        };

        require("jasmine-expect");
        require("require-dir")("./Presentation.Web/Tests/matchers");

        var reporters = require("jasmine-reporters");
        jasmine.getEnv().addReporter(new reporters.AppVeyorReporter({ batchSize: 1 }));
    },
    // json report
    resultJsonOutputFile: paths.e2eReport + '.json',

    mocks: {
        default: ['authorize'],
        dir: paths.source + '/Tests/mocks'
    }
};
