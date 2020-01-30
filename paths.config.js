"use strict";

var source = "Presentation.Web",
    typescriptOutput = source + "/typescriptOutput",
    typescriptOutputApp = typescriptOutput + "/app",
    typescriptOutputAppReport = typescriptOutput + "/appReport",
    sourceApp = source + "/app",
    sourceAppReport = source + "/appReport",
    sourceScript = source + "/Scripts",
    allJavaScript = [sourceApp + "/app.js", sourceApp + "/**/*.module.js" ,sourceApp + "/**/*.js"],
    allJavaScriptNoTests = [sourceApp + "/app.js", sourceApp + "/**/*.module.js", sourceApp + "/**/!(*.spec|*.po).js"],
    appTypeScriptOut = [typescriptOutputApp + "/app.js", typescriptOutputApp + "/**/*.module.js", typescriptOutputApp + "/**/!(*.spec|*.po).js"],
    appReportTypeScriptOut = typescriptOutput + "/appReport",
    bowerComponents = "bower_components",
    appMaps = sourceApp + "/**/*.js.map",
    npm = "node_modules",


    // e2e
    e2eFiles = source + "/Tests/**/*.e2e.spec.js",
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
    allJavaScriptNoTests: allJavaScriptNoTests,
    typescriptOutput: typescriptOutput,
    appTypeScriptOut: appTypeScriptOut,
    typescriptOutputAppReport: typescriptOutputAppReport,
    appReportTypeScriptOut: appReportTypeScriptOut,
    sourceAppReport: sourceAppReport,
    bowerComponents: bowerComponents,
    e2eFiles: e2eFiles,
    e2eReport: e2eReport,
    coverage: coverage,
    frontendCoverageReport: frontendCoverageReport,
    tempFrontendCoverageReport: tempFrontendCoverageReport,
    tempFiles: tempFiles,
    npm: npm
};
