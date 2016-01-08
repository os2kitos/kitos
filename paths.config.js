var source = "Presentation.Web",
    sourceApp = source + "/app",
    allJavaScript = [sourceApp + "/**/*.js"],
    allJavaScriptNoTests = [sourceApp + "/**/!(*.spec|*.po).js"],
    allTypeScript = [sourceApp + "/**/*.ts"],
    bundleDir = "./public",

    // dependency files of files to cover.
    unitDependencies = [
        source + "/Scripts/lodash.js",
        source + "/Scripts/jquery-2.1.4.js",
        source + "/Scripts/select2.js",
        source + "/Scripts/moment.js",
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
    allJavaScript: allJavaScript,
    allTypeScript: allTypeScript,
    bundleDir: bundleDir,
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
