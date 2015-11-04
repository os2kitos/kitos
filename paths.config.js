// Manage paths for all gulp actions
var source = 'Presentation.Web',
    sourceApp = source + '\\Scripts\\app',
    allJavaScript = [sourceApp + '\\**\\*.js'],
    allJavaScriptNoTests = [sourceApp + '\\**\\!(*.spec).js'],
    allTypeScript = [sourceApp + '\\**\\*.ts'],
    bundleDir = '.\\public',
    // Files to load in the browser before tests run.
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
    // Test files.
    karmaAppFiles = [
        source + '/Scripts/app/**/*.js'
    ],
    karma = karmaBrowserLibs.concat(karmaAppFiles);

module.exports = {
    source: source,
    sourceApp: sourceApp,
    allJavaScript: allJavaScript,
    allTypeScript: allTypeScript,
    bundleDir: bundleDir,
    karma: karma
};
