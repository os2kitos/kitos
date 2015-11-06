var paths = require('../paths.config.js');

exports.config = {
    // Path to the selenium server jar. Update version number accordingly!
    seleniumServerJar: paths.seleniumServerJar,

    // select all end to end tests
    specs: paths.e2eFiles,

    resultJsonOutputFile: paths.e2eReport
};
