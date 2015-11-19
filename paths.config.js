var source = 'Presentation.Web',
    sourceApp = source + '/Scripts/app',
    allJavaScript = [sourceApp + '/**/*.js'],
    allJavaScriptNoTests = [sourceApp + '/**/!(*.spec).js'],
    allTypeScript = [sourceApp + '/**/*.ts'],
    bundleDir = './public',

    // dependency files of files to cover.
    karmaBrowserLibs = [
        source + '/Scripts/lodash.js',
        source + '/Scripts/jquery-2.1.4.js',
        source + '/Scripts/select2.js',
        source + '/Scripts/moment.js',
        source + '/Scripts/bootstrap.js',
        source + '/Scripts/angular.js',
        source + '/Scripts/i18n/angular-locale_da-dk.js',
        source + '/Scripts/angular-animate.js',
        source + '/Scripts/angular-sanitize.js',
        source + '/Scripts/angular-ui-router.js',
        source + '/Scripts/angular-ui/ui-bootstrap.js',
        source + '/Scripts/angular-ui/ui-bootstrap-tpls.js',
        source + '/Scripts/ui-select2.js',
        source + '/Scripts/notify/*.js',
        source + '/Scripts/angular-ui-util/ui-utils.js',
    ],
    // files to cover.
    karmaAppFiles = [
        source + '/Scripts/app/**/*.js'
    ],
    karma = karmaBrowserLibs.concat(karmaAppFiles),

    // all e2e tests.
    e2eFiles = ['Tests/**/*pag.spec.js', 'Tests/**/*e2e.spec.js'],
    e2eReport = 'results-protractor',
    seleniumServerJar = '../node_modules/protractor/selenium/selenium-server-standalone-2.47.1.jar';

module.exports = {
    source: source,
    sourceApp: sourceApp,
    allJavaScript: allJavaScript,
    allTypeScript: allTypeScript,
    bundleDir: bundleDir,
    karma: karma,
    e2eFiles: e2eFiles,
    e2eReport: e2eReport,
    seleniumServerJar: seleniumServerJar
};
