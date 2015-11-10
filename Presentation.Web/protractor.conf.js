var paths = require('../paths.config.js');

exports.config = {
    // path to the selenium server jar. Update version number accordingly!
    seleniumServerJar: paths.seleniumServerJar,

    // select all end to end tests
    specs: paths.e2eFiles,

    // output results to xml
    resultJsonOutputFile: paths.e2eReport,

    // increase timeout to allow AppVeyor to rebuild database on first instantiation.
    allScriptsTimeout: 30000
};
