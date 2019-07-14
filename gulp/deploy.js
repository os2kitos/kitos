var gulp = require("gulp");
var concat = require("gulp-concat");
var uglify = require("gulp-uglify");
var del = require("del");
var minifyCSS = require("gulp-minify-css");
var bower = require("gulp-bower");
var sourcemaps = require("gulp-sourcemaps");
var runSequence = require('run-sequence');
var ts = require('gulp-typescript');
var htmlreplace = require('gulp-html-replace');
var debug = require('gulp-debug');
var rename = require('gulp-rename');
var file = file = require('gulp-file');
var less = require('gulp-less');
var paths = require("../paths.config.js");
var config = require("../bundle.config.js");
var tsProject = ts.createProject('tsconfig.json');

//Synchronously delete the output script file(s)
gulp.task("clean-js-and-maps", function () {
    return del(paths.typescriptOutput, paths.allJavaScriptNoTests, paths.appMaps);
});

// create css bundled file
gulp.task("css", ["clean-styles"], function () {
    return gulp.src(config.libraryStylesSrc.concat(config.customCssSrc))
        .pipe(sourcemaps.init())
        .pipe(less())
        .pipe(concat(config.cssBundle))
        .pipe(gulp.dest(config.cssDest))
        .pipe(minifyCSS())
        .pipe(concat(config.cssBundleMin))
        .pipe(sourcemaps.write(config.maps))
        .pipe(gulp.dest(config.cssDest));
});

gulp.task('typescript', function () {
    tsResult = tsProject.src()
        .pipe(tsProject());

    return tsResult.js.pipe(gulp.dest(paths.source));
});

gulp.task("clean-script-bundles", function () {
    return del([
        config.script(config.libraryBundle),
        config.script(config.angularBundle),
        config.script(config.appBundle),
        config.script(config.appReportBundle),
    ]);
});

gulp.task("clean-scripts", ["clean-script-bundles", "clean-js-and-maps"]);

// create external library bundled file
gulp.task("library-bundle", function () {
    return gulp.src(config.librarySrc)
        .pipe(sourcemaps.init())
        .pipe(concat(config.libraryBundle))
        .pipe(sourcemaps.write(config.maps))
        .pipe(gulp.dest(paths.sourceScript));
});

// create angular library bundled file
gulp.task("angular-bundle", function () {
    return gulp.src(config.angularSrc)
        .pipe(sourcemaps.init())
        .pipe(concat(config.angularBundle))
        .pipe(sourcemaps.write(config.maps))
        .pipe(gulp.dest(paths.sourceScript));
});

// create app bundled file
gulp.task("app-bundle", function () {
    return gulp.src(config.appSrc)
        .pipe(sourcemaps.init())
        .pipe(concat(config.appBundle))
        .pipe(uglify())
        .pipe(sourcemaps.write(config.maps))
        .pipe(gulp.dest(paths.sourceScript));
});

// create app report bundled file
gulp.task("appReport-bundle", function () {
    return gulp.src(config.appReportSrc)
        .pipe(sourcemaps.init())
        .pipe(concat(config.appReportBundle))
        .pipe(uglify())
        .pipe(sourcemaps.write(config.maps))
        .pipe(gulp.dest(paths.sourceScript))
});

// delete style output folders
gulp.task("clean-styles", function () {
    return del([
        config.fontDest,
        config.cssDest,
        config.cssDest + "/" + config.maps
    ]);
});

// copy assets
gulp.task("assets", ["clean-styles"], function () {
    return gulp.src(config.assetsSrc)
        .pipe(gulp.dest(config.cssDest));
});

// copy fonts
gulp.task("fonts", ["clean-styles"], function () {
    return gulp.src(config.fontSrc)
        .pipe(gulp.dest(config.fontDest));
});

// copy tinyMCE fonts
gulp.task("tinyMCEFonts", ["clean-styles"], function () {
    return gulp.src(config.tinyMCEFontSrc)
        .pipe(gulp.dest(config.tinyMCEFontDest));
});

gulp.task("tinyMCEFixCss", ["clean-styles"], function () {
    return file("content.min.css", "//Dummy file from gulp", { src: true })
    .pipe(gulp.dest(paths.sourceScript + "/skins/lightgray"))
    .pipe(rename("skin.min.css"))
    .pipe(gulp.dest(paths.sourceScript + "/skins/lightgray"));
});

gulp.task("tinyMCEFixLang", ["clean-styles"], function () {
    return file("da.js", "//Dummy file from gulp", { src: true })
    .pipe(gulp.dest(paths.sourceScript + "/langs"));
});

// restore all bower packages
gulp.task("bower-restore", function () {
    return bower(paths.bowerComponents);
});

// bundle, minify and copy styles, fonts and assets
gulp.task("styles", ["css", "assets", "fonts", "tinyMCEFonts", "tinyMCEFixCss", "tinyMCEFixLang"]);

// run bundle tasks
gulp.task("scripts", ["app-bundle", "library-bundle", "angular-bundle"]);

// bundle and deploy scripts and styles
gulp.task("deploy", function (callback) {
    runSequence("clean-script-bundles", "scripts", "styles", "clean-js-and-maps", callback);
});

// bundle and deploy scripts and styles
gulp.task("deploy-prod", function (callback) {
    runSequence("clean-scripts", "typescript", "library-bundle", "angular-bundle", "app-bundle", "appReport-bundle", "styles", "clean-js-and-maps", callback);
});
