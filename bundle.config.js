var paths = require("./paths.config.js");

module.exports = {
    // library script bundle
    // not minified!
    librarySrc: [
        npm("/lodash/lodash.min.js"),
        npm("/jquery/dist/jquery.min.js"),
        npm("/select2/select2.js"),
        npm("/select2/select2_locale_da.js"),
        npm("/moment/min/moment.min.js"),
        npm("/json-fn/jsonfn.min.js"),
        npm("/tinymce/tinymce.js"),
        npm("/tinymce/plugins/image/plugin.min.js"),
        npm("/tinymce/plugins/code/plugin.min.js"),
        npm("/tinymce/plugins/link/plugin.min.js"),
        npm("/tinymce/themes/modern/theme.min.js"),
        npm("/js-sha256/src/sha256.js")
    ],
    libraryBundle: "library-bundle.min.js",

    libraryStylesSrc: [
        npm("/select2/select2.css"),
        npm("/select2-bootstrap-css/select2-bootstrap.min.css"),
        npm("/angular-loading-bar/build/loading-bar.min.css"),
        npm("/angular-ui-tree/dist/angular-ui-tree.min.css"),
        npm("/tinymce/skins/lightgray/skin.min.css"),
        npm("/tinymce/skins/lightgray/content.min.css")
    ],

    // angular script bundle
    // not minified
    angularSrc: [
        npm("/angular/angular.min.js"),
        npm("/angular-i18n/angular-locale_da-dk.js"),
        npm("/angular-animate/angular-animate.min.js"),
        npm("/angular-sanitize/angular-sanitize.min.js"),
        npm("/angular-ui-router/release/angular-ui-router.min.js"),
        npm("/angular-ui-bootstrap/dist/ui-bootstrap-tpls.js"),
        npm("/angular-ui-select2/src/select2.js"),
        npm("/angular-loading-bar/build/loading-bar.min.js"),
        npm("/angularjs-dropdown-multiselect/dist/angularjs-dropdown-multiselect.min.js"),
        npm("/angular-confirm/angular-confirm.min.js"),
        npm("/angular-messages/angular-messages.min.js"),
        npm("/angular-ui-tree/dist/angular-ui-tree.min.js"),
        npm("/angular-ui-tinymce/src/tinymce.js"),
        npm("/angular-route/angular-route.js"),
        npm("/ngstorage/ngstorage.js"),
        npm("/angular-base64/angular-base64.js"),
        npm("/angular-cookies/angular-cookies.min.js")
    ],
    angularBundle: "angular-bundle.min.js",

    // app script bundle
    appSrc: paths.allJavaScriptNoTests,
    appBundle: "app-bundle.min.js",

    // font bundle
    fontSrc: [
        npm("/bootstrap/dist/fonts/*.*"),
        npm("/font-awesome/fonts/*.*")
    ],

    tinyMCEFontSrc: [
        npm("/tinymce/skins/lightgray/fonts/*.*")
    ],

    // assets
    assetsSrc: [
        npm("/select2/*.png"),
        npm("/select2/*.gif")
    ],

    // custom style bundle
    customCssSrc: [
        content("/less/styles.less") 
    ],
    cssBundle: "app.css",
    cssBundleMin: "app.min.css",

    fontDest: content("/fonts"),
    tinyMCEFontDest: content("/css/fonts"),
    cssDest: content("/css"),
    maps: "maps",

    script: script,
    content: content,
    npm: npm
};

// path helper functions
function script(file) {
    return paths.sourceScript + "/" + file;
}

function content(file) {
    return paths.source + "/Content" + file;
}

function npm(file) {
    return paths.npm + "/" + file;
}