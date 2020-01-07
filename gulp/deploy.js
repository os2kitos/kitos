"use strict";

const { src, dest, series, parallel} = require("gulp");
var concat = require("gulp-concat");
var uglify = require("gulp-uglify");
var del = require("del");
var minifyCSS = require("gulp-minify-css");
var sourcemaps = require("gulp-sourcemaps");
var ts = require('gulp-typescript');
var rename = require('gulp-rename');
var file = file = require('gulp-file');
var less = require('gulp-less');
var paths = require("../paths.config.js");
var config = require("../bundle.config.js");
var tsProject = ts.createProject('tsconfig.json');

//Synchronously delete the output script file(s)
const cleanJsAndMaps = function(cb) {
    return del(paths.typescriptOutput, paths.allJavaScriptNoTests, paths.appMaps);
};

// create css bundled file
const css = function(cb) {
    return src(config.libraryStylesSrc.concat(config.customCssSrc))
        .pipe(sourcemaps.init())
        .pipe(less())
        .pipe(concat(config.cssBundle))
        .pipe(dest(config.cssDest))
        .pipe(minifyCSS())
        .pipe(concat(config.cssBundleMin))
        .pipe(sourcemaps.write(config.maps))
        .pipe(dest(config.cssDest));
};

const typescript = function(cb) {
    const tsResult = tsProject.src()
        .pipe(tsProject());

    return tsResult.js.pipe(dest(paths.source));
};


const cleanScriptBundles = function(cb) {
    return del([
        config.script(config.libraryBundle),
        config.script(config.angularBundle),
        config.script(config.appBundle),
        config.script(config.appReportBundle),
    ]);
};

const cleanScripts = parallel(cleanScriptBundles, cleanJsAndMaps);

// create external library bundled file
const libraryBundle = function(cb) {
    return src(config.librarySrc)
        .pipe(sourcemaps.init())
        .pipe(concat(config.libraryBundle))
        .pipe(sourcemaps.write(config.maps))
        .pipe(dest(paths.sourceScript));
};

// create angular library bundled file
const angularBundle = function(cb) {
    return src(config.angularSrc)
        .pipe(sourcemaps.init())
        .pipe(concat(config.angularBundle))
        .pipe(sourcemaps.write(config.maps))
        .pipe(dest(paths.sourceScript));
};

// create app bundled file
const appBundle = function(cb) {
    return src(config.appSrc)
        .pipe(sourcemaps.init())
        .pipe(concat(config.appBundle))
        .pipe(uglify())
        .pipe(sourcemaps.write(config.maps))
        .pipe(dest(paths.sourceScript));
};

// create app report bundled file
const appReportBundle = function(cb) {
    return src(config.appReportSrc)
        .pipe(sourcemaps.init())
        .pipe(concat(config.appReportBundle))
        .pipe(uglify())
        .pipe(sourcemaps.write(config.maps))
        .pipe(dest(paths.sourceScript));
};

// delete style output folders
const cleanStyles = function(cb) {
    return del([
        config.fontDest,
        config.cssDest,
        config.cssDest + "/" + config.maps
    ]);
};

// copy assets
const assets = function(cb) {
    return src(config.assetsSrc)
        .pipe(dest(config.cssDest));
};

// copy fonts
const fonts = function(cb) {
    return src(config.fontSrc)
        .pipe(dest(config.fontDest));
};

// copy tinyMCE fonts
const tinyMCEFonts = function(cb) {
    return src(config.tinyMCEFontSrc)
        .pipe(dest(config.tinyMCEFontDest));
};

const tinyMCEFixCss = function(cb) {
    return file("content.min.css", "//Dummy file from gulp", { src: true })
    .pipe(dest(paths.sourceScript + "/skins/lightgray"))
    .pipe(rename("skin.min.css"))
    .pipe(dest(paths.sourceScript + "/skins/lightgray"));
};

const tinyMCEFixLang = function(cb) {
    return file("da.js", "//Dummy file from gulp", { src: true })
    .pipe(dest(paths.sourceScript + "/langs"));
};

// bundle, minify and copy styles, fonts and assets
const styles = series(cleanStyles, parallel(css, assets, fonts), parallel(tinyMCEFonts, tinyMCEFixCss, tinyMCEFixLang));
//gulp.task("styles", ["css", "assets", "fonts", "", "", ""]);

// run bundle tasks
const scripts = series(cleanScriptBundles, parallel(appBundle, libraryBundle, angularBundle, appReportBundle));
//gulp.task("scripts", ["app-bundle", "library-bundle", "angular-bundle"]);

//// bundle and deploy scripts and styles
exports.deployProd = series(typescript, parallel(scripts, styles), cleanJsAndMaps);
//gulp.task("deploy-prod", function (callback) {
//    runSequence("clean-scripts", "typescript", "library-bundle", "angular-bundle", "app-bundle", "appReport-bundle", "styles", "clean-js-and-maps", callback);
//});


exports.clean = parallel(cleanStyles, cleanScripts);