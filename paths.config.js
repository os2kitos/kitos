var source = __dirname + '\\Presentation.Web',
    sourceApp = source + '\\Scripts\\app',
    allJavaScript = [sourceApp + '\\**\\*.js'],
    allJavaScriptNoTests = [sourceApp + '\\**\\!(*.spec).js'],
    allTypeScript = [sourceApp + '\\**\\*.ts'],
    bundleDir = '.\\bundle',
    karmaFiles = [
        source + '\\Scripts\\lodash.js',
        source + '\\Scripts\\jquery-2.1.4.js',
        source + '\\Scripts\\select2.js',
        source + '\\Scripts\\moment.js',
        source + '\\Scripts\\bootstrap.js',
        source + '\\Scripts\\angular.js',
        source + '\\Scripts\\i18n\\angular-locale_da-dk.js',
        source + '\\Scripts\\angular-animate.js',
        source + '\\Scripts\\angular-sanitize.js',
        source + '\\Scripts\\angular-ui-router.js',
        source + '\\Scripts\\angular-ui\\ui-bootstrap.js',
        source + '\\Scripts\\angular-ui\\ui-bootstrap-tpls.js',
        source + '\\Scripts\\ui-select2.js',
        source + '\\Scripts\\notify\\*.js',
        source + '\\Scripts\\angular-ui-util\\ui-utils.js',
        source + '\\Scripts\\app\\**\\*.js'
    ];


module.exports = {
    source: source,
    sourceApp: sourceApp,
    allJavaScript: allJavaScript,
    allTypeScript: allTypeScript,
    bundleDir: bundleDir,
    karmaFiles: karmaFiles
};
