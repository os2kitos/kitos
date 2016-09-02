var paths = require("./paths.config.js");

module.exports = {
    // library script bundle
    // not minified!
    librarySrc: [
        bower("/lodash/dist/lodash.min.js"),
        bower("/jquery/dist/jquery.min.js"),
        bower("/select2/select2.min.js"),
        bower("/select2/select2_locale_da.js"),
        bower("/moment/min/moment.min.js"),
        bower("/jsonfn-bower/jsonfn.min.js")
    ],
    libraryBundle: "library-bundle.min.js",

    libraryStylesSrc: [
        bower("/bootstrap/dist/css/bootstrap.min.css"),
        bower("/font-awesome/css/font-awesome.min.css"),
        bower("/select/dist/select.css"),
        bower("/select2-bootstrap-css/select2-bootstrap.min.css"),
        bower("/angular-loading-bar/build/loading-bar.min.css")
    ],

    // stimulsoftSrc: [
    //     script("/stimulsoft/stimulsoft.reports.js"),
    //     script("/stimulsoft/stimulsoft.viewer.js"),
    //     script("/stimulsoft/stimulsoft.designer.js")
    // ],
    // stimulsoftBundle: "stimulsoft-bundle.js",

    // angular script bundle
    // not minified
    angularSrc: [
        bower("/angular/angular.min.js"),
        bower("/angular-i18n/angular-locale_da-dk.js"),
        bower("/angular-animate/angular-animate.min.js"),
        bower("/angular-sanitize/angular-sanitize.min.js"),
        bower("/angular-ui-router/release/angular-ui-router.min.js"),
        bower("/angular-bootstrap/ui-bootstrap-tpls.min.js"),
        bower("/angular-ui-select/dist/select.js"),
        bower("/angular-loading-bar/build/loading-bar.min.js"),
        bower("/angularjs-dropdown-multiselect/dist/angularjs-dropdown-multiselect.min.js")
    ],
    angularBundle: "angular-bundle.min.js",

    // app script bundle
    appSrc: paths.allJavaScriptNoTests,
    appBundle: "app-bundle.min.js",

    // font bundle
    fontSrc: [
        bower("/bootstrap/dist/fonts/*.*"),
        bower("/font-awesome/fonts/*.*")
    ],

    // assets
    assetsSrc: [
        bower("/select2/*.png"),
        bower("/select2/*.gif")
    ],

    // custom style bundle
    customCssSrc: [
        content("custom-ui-select.css"),
        content("/notify/notify.css"),
        content("/kitos.css")
    ],
    cssBundle: "app.css",
    cssBundleMin: "app.min.css",

    fontDest: content("/fonts"),
    cssDest: content("/css"),
    maps: "maps",

    script: script,
    content: content,
    bower: bower
};

// path helper functions
function script(file) {
    return paths.sourceScript + "/" + file;
}

function content(file) {
    return paths.source + "/Content" + file;
}

function bower(file) {
    return paths.bowerComponents + "/" + file;
}
