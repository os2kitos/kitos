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
        bower("/jsonfn-bower/jsonfn.min.js"),
        bower("/tinymce/tinymce.js"),
        bower("/tinymce/plugins/image/plugin.min.js"),
        bower("/tinymce/plugins/code/plugin.min.js"),
        bower("/tinymce/plugins/link/plugin.min.js"),
        bower("/tinymce/themes/modern/theme.min.js")
    ],
    libraryBundle: "library-bundle.min.js",

    libraryStylesSrc: [
        bower("/select2/select2.css"),
        bower("/select2-bootstrap-css/select2-bootstrap.min.css"),
        bower("/angular-loading-bar/build/loading-bar.min.css"),
        bower("/angular-ui-tree/dist/angular-ui-tree.min.css"),
        bower("/tinymce/skins/lightgray/skin.min.css"),
        bower("/tinymce/skins/lightgray/content.min.css")
    ],

    // angular script bundle
    // not minified
    angularSrc: [
        bower("/angular/angular.min.js"),
        bower("/angular-i18n/angular-locale_da-dk.js"),
        bower("/angular-animate/angular-animate.min.js"),
        bower("/angular-sanitize/angular-sanitize.min.js"),
        bower("/angular-ui-router/release/angular-ui-router.min.js"),
        bower("/angular-bootstrap/ui-bootstrap-tpls.min.js"),
        bower("/angular-ui-select2/src/select2.js"),
        bower("/angular-loading-bar/build/loading-bar.min.js"),
        bower("/angularjs-dropdown-multiselect/dist/angularjs-dropdown-multiselect.min.js"),
        bower("/angular-confirm-modal/angular-confirm.min.js"),
        bower("/angular-messages/angular-messages.min.js"),
        bower("/angular-ui-tree/dist/angular-ui-tree.min.js"),
        bower("/angular-ui-tinymce/src/tinymce.js"),
        bower("/angular-route/angular-route.js"),
        bower("/ngstorage/ngstorage.js"),
        bower("/angular-base64/angular-base64.js"),
        bower("/oidc-angular/oidc-angular.js"),
        npm("/angular-cookies/angular-cookies.min.js")
    ],
    angularBundle: "angular-bundle.min.js",

    // app script bundle
    appSrc: paths.allJavaScriptNoTests,
    appBundle: "app-bundle.min.js",

    // app script bundle
    appReportSrc: [
        app("models/api/organization-role.js"),
        appReport("reportApp.module.js"),
        app("shared/notify/notify.module.js"),
        app("shared/notify/notify.directive.js"),
        app("shared/notify/notify.factory.js"),
        appReport("services/stimulsoftService.js"),
        app("services/ReportService.js"),
        app("services/userServices.js"),
        app("interceptors/csrfRequestInterceptor.js"),
        appReport("report-viewer.controller.js")
    ],
    appReportBundle: "appReport-bundle.min.js",

    // font bundle
    fontSrc: [
        bower("/bootstrap/dist/fonts/*.*"),
        bower("/font-awesome/fonts/*.*")
    ],

    tinyMCEFontSrc: [
        bower("/tinymce/skins/lightgray/fonts/*.*")
    ],

    // assets
    assetsSrc: [
        bower("/select2/*.png"),
        bower("/select2/*.gif")
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
    bower: bower,
    npm: npm
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

function npm(file) {
    return paths.npm + "/" + file;
}

function app(file) {
    return "Presentation.Web/app/" + file;
}

function appReport(file) {
    return "Presentation.Web/appReport/" + file;
}


