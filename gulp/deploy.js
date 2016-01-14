
var gulp = require("gulp");
var gutil = require('gulp-util');
var concat = require("gulp-concat");
var uglify = require("gulp-uglify");
var del = require("del");
var minifyCSS = require("gulp-minify-css");
var copy = require("gulp-copy");
var bower = require("gulp-bower");
var sourcemaps = require("gulp-sourcemaps");
var paths = require("../paths.config.js");


var config = {
    // library script bundle
    // not minified!
    librarySrc: [
        paths.bowerComponents + "/lodash/lodash.min.js",
        paths.bowerComponents + "/jquery/dist/jquery.min.js",
        paths.bowerComponents + "/select2/select2.min.js",
        paths.bowerComponents + "/moment/min/moment.min.js",
        paths.bowerComponents + "/bootstrap/dist/js/bootstrap.min.js",
        paths.bowerComponents + "/jsonfn-bower/jsonfn.min.js"
    ],
    libraryBundle: "library-bundle.min.js",

    // angular script bundle
    // not minified
    angularSrc: [
        paths.bowerComponents + "/angular/angular.min.js",
        paths.bowerComponents + "/angular-i18n/angular-locale_da-dk.js",
        paths.bowerComponents + "/angular-animate/angular-animate.min.js",
        paths.bowerComponents + "/angular-sanitize/angular-sanitize.min.js",
        paths.bowerComponents + "/angular-ui-router/release/angular-ui-router.min.js",
        paths.bowerComponents + "/angular-bootstrap/ui-bootstrap-tpls.min.js",
        paths.bowerComponents + "/angular-ui-select2/src/select2.js",
        paths.bowerComponents + "/angular-loading-bar/build/loading-bar.min.js",
        paths.sourceScript + "/notify/*.js",
        paths.sourceScript + "/angular-ui-util/*min.js"
    ],
    angularBundle: "angular-bundle.min.js",

    // app script bundle
    appSrc: paths.allJavaScriptNoTests,
    appBundle: "app-bundle.min.js",

    // library style bundle
    libraryStylesSrc: [
        paths.bowerComponents + "/bootstrap/dist/css/bootstrap.min.css",
        paths.bowerComponents + "/font-awesome/css/font-awesome.min.css",
        paths.bowerComponents + "/select2/select2.css",
        paths.bowerComponents + "/select2/select2-bootstrap.css",
        paths.bowerComponents + "/angular-loading-bar/loading-bar.min.css"
    ],

    // font bundle
    fontSrc: [
        paths.bowerComponents + "/bootstrap/dist/fonts/*.*",
        paths.bowerComponents + "/font-awesome/fonts/*.*"
    ],

    // custom style bundle
    customCssSrc: [
        paths.source + "/Content/notify/notify.css",
        paths.source + "/Content/kitos.css"
    ],
    cssBundle: "app.css",
    cssBundleMin: "app.min.css",

    // assets
    assetsSrc: [
        paths.bowerComponents + "/select2/*.png",
        paths.bowerComponents + "/select2/*.gif"
    ],

    fontDest: paths.source + "/Content/dist/fonts",
    cssDest: paths.source + "/Content/dist/css"
}

function script(file) {
    return paths.sourceScript + "/" + file;
}

//Synchronously delete the output script file(s)
gulp.task("clean-scripts", function () {
    return del([
        script(config.libraryBundle),
        script(config.angularBundle),
        script(config.appBundle)
    ]);
});

// create external library bundled file
gulp.task("library-bundle", ["clean-scripts", "bower-restore"], function () {
    return gulp.src(config.librarySrc)
     .pipe(concat(config.libraryBundle))
     .pipe(gulp.dest(paths.sourceScript));
});

// create angular library bundled file
gulp.task("angular-bundle", ["clean-scripts", "bower-restore"], function () {
    return gulp.src(config.angularSrc)
     .pipe(sourcemaps.init())
     .pipe(concat(config.angularBundle))
     .pipe(sourcemaps.write("maps"))
     .pipe(gulp.dest(paths.sourceScript));
});

// create app bundled file
gulp.task("app-bundle", ["clean-scripts"], function () {
    return gulp.src(config.appSrc)
     .pipe(sourcemaps.init())
     .pipe(uglify())
     .pipe(concat(config.appBundle))
     .pipe(sourcemaps.write("maps"))
     .pipe(gulp.dest(paths.sourceScript));
});

// run bundle tasks
gulp.task("scripts", ["library-bundle", "angular-bundle", "app-bundle"]);

// delete style output folders
gulp.task("clean-styles", function () {
    return del([
        config.fontDest,
        config.cssDest
    ]);
});

gulp.task("assets", ["clean-styles", "bower-restore"], function() {
    return gulp.src(config.assetsSrc)
        .pipe(gulp.dest(config.cssDest));
});

gulp.task("css", ["clean-styles", "bower-restore"], function () {
    return gulp.src(config.libraryStylesSrc.concat(config.customCssSrc))
        .pipe(concat(config.cssBundle))
        .pipe(gulp.dest(config.cssDest))
        .pipe(minifyCSS())
        .pipe(concat(config.cssBundleMin))
        .pipe(gulp.dest(config.cssDest));
});

gulp.task("fonts", ["clean-styles", "bower-restore"], function () {
    return gulp.src(config.fontSrc)
        .pipe(gulp.dest(config.fontDest));
});

// Combine and minify css files and output fonts
gulp.task("styles", ["css", "assets", "fonts"]);

//Restore all bower packages
gulp.task("bower-restore", function () {
    return bower()
        .pipe(gulp.dest(paths.bowerComponents));
});

//Set a default tasks
gulp.task("deploy", ["scripts", "styles"]);
