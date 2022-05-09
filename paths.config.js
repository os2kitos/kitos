"use strict";

var source = "Presentation.Web",
    typescriptOutput = source + "/typescriptOutput",
    typescriptOutputApp = typescriptOutput + "/app",
    sourceApp = source + "/app",
    sourceScript = source + "/Scripts",
    allJavaScript = [sourceApp + "/app.js", sourceApp + "/**/*.module.js", sourceApp + "/**/!(tinyMCE_lang_da).js"],
    allJavaScriptNoTests = [sourceApp + "/app.js", sourceApp + "/**/*.module.js", sourceApp + "/**/!(*.spec|*.po|*tinyMCE_lang_da).js"],
    appTypeScriptOut = [typescriptOutputApp + "/app.js", typescriptOutputApp + "/**/*.module.js", typescriptOutputApp + "/**/!(*.spec|*.po).js"],
    appMaps = sourceApp + "/**/*.js.map",
    npm = "node_modules",


    // e2e
    e2eFiles = source + "/Tests/**/*.e2e.spec.js",
    e2eParallelFiles = source + "/Tests/01_Parallel/**/*.e2e.spec.js",
    e2eSequentialFiles = source + "/Tests/02_Sequential/**/*.e2e.spec.js",
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
    e2eFiles: e2eFiles,
    e2eParallelFiles: e2eParallelFiles,
    e2eSequentialFiles: e2eSequentialFiles,
    e2eReport: e2eReport,
    coverage: coverage,
    frontendCoverageReport: frontendCoverageReport,
    tempFrontendCoverageReport: tempFrontendCoverageReport,
    tempFiles: tempFiles,
    npm: npm
};