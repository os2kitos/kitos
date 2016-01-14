var source = "Presentation.Web",
    sourceApp = source + "/app",
    sourceScript = source + "/Scripts",
    allJavaScript = [sourceApp + "/app.js", sourceApp + "/**/*.js"],
    allJavaScriptNoTests = [sourceApp + "/**/!(*.spec|*.po).js"],
    allTypeScript = [sourceApp + "/**/*.ts"],
    bundleDir = "./public",
    bowerComponents = "bower_components",

    // dependency files of files to unit test
    unitDependencies = [
        source + "/Scripts/lodash.js",
        source + "/Scripts/jquery-2.1.4.js",
        source + "/Scripts/select2.js",
        source + "/Scripts/moment.js",
        source + '/Scripts/jsonfn.js',
        source + "/Scripts/bootstrap.js",
        source + "/Scripts/angular.js",
        source + "/Scripts/i18n/angular-locale_da-dk.js",
        source + "/Scripts/angular-animate.js",
        source + "/Scripts/angular-sanitize.js",
        source + "/Scripts/angular-mocks.js",
        source + "/Scripts/angular-ui-router.js",
        source + "/Scripts/angular-ui/ui-bootstrap.js",
        source + "/Scripts/angular-ui/ui-bootstrap-tpls.js",
        source + "/Scripts/ui-select2.js",
        source + "/Scripts/notify/*.js",
        source + "/Scripts/angular-ui-util/ui-utils.js"
    ],

    // unit
    karmaConf = __dirname + "/karma.conf.js",
    unitSource = [
        sourceApp + "/app.js",
        sourceApp + "/**/!(*.po|*.spec).js",
        sourceApp + "/**/*.spec.js"
    ],
    unit = unitDependencies.concat(unitSource),

    // e2e
    e2eFiles = source + "/Tests/**/*.e2e.spec.js",
    e2eSuites = {
        home: source + "/Tests/home.e2e.spec.js",
        itProject: source + "/Tests/ItProject/**/*e2e.spec.js"
    },
    e2eReport = "results-protractor",

    // coverage
    coverage = "coverage",
    frontendCoverageReport = "frontend.json",
    tempFrontendCoverageReport = "temp-coverage.json",

    tempFiles = [ e2eReport + ".json", coverage, bundleDir, source + "/Tests/**/*.js" ];

module.exports = {
    source: source,
    sourceApp: sourceApp,
    sourceScript: sourceScript,
    allJavaScript: allJavaScript,
    allTypeScript: allTypeScript,
    allJavaScriptNoTests: allJavaScriptNoTests,
    bundleDir: bundleDir,
    bowerComponents: bowerComponents,
    unit: unit,
    e2eFiles: e2eFiles,
    e2eSuites: e2eSuites,
    e2eReport: e2eReport,
    karmaConf: karmaConf,
    coverage: coverage,
    frontendCoverageReport: frontendCoverageReport,
    tempFrontendCoverageReport: tempFrontendCoverageReport,
    tempFiles: tempFiles
};
