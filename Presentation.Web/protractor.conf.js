var paths = require('../paths.config.js');

exports.config = {
    // path to the selenium server jar. Update version number accordingly!
    seleniumServerJar: paths.seleniumServerJar,

    // select all end to end tests
    suites: {
        home: 'Tests/home.e2e.spec.js',
        itProject: 'Tests/ItProject/**/*.e2e.spec.js'
    },

    // output results to xml
    resultJsonOutputFile: paths.e2eReport,

    // increase timeout to allow AppVeyor to rebuild database on first instantiation.
    allScriptsTimeout: 30000,
    baseUrl: 'https://localhost:44300',

    onPrepare: function () {
        require('protractor-http-mock').config = {
            rootDirectory: __dirname
        }
    },

    mocks: {
        default: ['authorize'],
        dir: 'Tests/mocks'
    }
};
