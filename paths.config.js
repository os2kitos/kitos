"use strict";

var source = "Presentation.Web",
    sourceApp = source + "/app",
    sourceScript = source + "/Scripts",
    allJavaScript = [sourceApp + "/app.js", sourceApp + "/**/*.js"],
    allJavaScriptNoTests = sourceApp + "/**/!(*.spec|*.po).js",
    allTypeScript = [sourceApp + "/**/*.ts"],
    bowerComponents = "bower_components",
    appMaps = sourceApp + "/**/*.js.map",

    // dependency files of files to unit test
    unitDependencies = [
        "bower_components/lodash/lodash.js",
        "bower_components/jquery/dist/jquery.js",
        "bower_components/moment/moment.js",
        "bower_components/jsonfn-bower/jsonfn.js",
        "bower_components/angular/angular.js",
        "bower_components/angular-i18n/angular-locale_da-dk.js",
        "bower_components/angular-animate/angular-animate.js",
        "bower_components/angular-sanitize/angular-sanitize.js",
        "bower_components/angular-mocks/angular-mocks.js",
        "bower_components/angular-ui-router/release/angular-ui-router.js",
        "bower_components/angular-bootstrap/ui-bootstrap.js",
        "bower_components/angular-bootstrap/ui-bootstrap-tpls.js",
        "bower_components//angular-ui-select/dist/select.js"        
    ],

    // unit
    karmaConf = __dirname + "/karma.conf.js",
    unitSource = [
        sourceApp + "/shared/notify/notify.module.js",
        sourceApp + "/app.js",
        sourceApp + "/**/!(*.po|*.spec).js",
        sourceApp + "/**/*.spec.js"
    ],
    unit = unitDependencies.concat(unitSource),

    // e2e
    e2eFiles = source + "/Tests/**/*.e2e.spec.js",
    e2eSuites = {
        home: source + "/Tests/home.e2e.spec.js",
        itProject: source + "/Tests/ItProject/**/*e2e.spec.js",
        itSystem: source + "/Tests/it-system/**/*e2e.spec.js",
        itContract: source + "/Tests/it-contract/**/*e2e.spec.js"
    },

    e2eReport = "results-protractor",

    // coverage
    coverage = "coverage",
    frontendCoverageReport = "frontend.json",
    tempFrontendCoverageReport = "temp-coverage.json",

    tempFiles = [e2eReport + ".json", coverage];

module.exports = {
    source: source,
    sourceApp: sourceApp,
    appMaps: appMaps,
    sourceScript: sourceScript,
    allJavaScript: allJavaScript,
    allTypeScript: allTypeScript,
    allJavaScriptNoTests: allJavaScriptNoTests,
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
