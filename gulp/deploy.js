"use strict";

const { src, dest, series, parallel} = require("gulp");
var concat = require("gulp-concat");
var uglify = require("gulp-uglify");
var del = require("del");
var minifyCss = require("gulp-clean-css");
var sourceMaps = require("gulp-sourcemaps");
var ts = require("gulp-typescript");
var rename = require("gulp-rename");
var file = file = require("gulp-file");
var less = require("gulp-less");
var paths = require("../paths.config.js");
var config = require("../bundle.config.js");
var tsProject = ts.createProject("tsconfig.json");

//Synchronously delete the output script file(s)
const cleanJsAndMaps = function (callBack) {
    return del(paths.typescriptOutput, paths.allJavaScriptNoTests, paths.appMaps);
};

// create css bundled file
const css = function (callBack) {
    return src(config.libraryStylesSrc.concat(config.customCssSrc))
        .pipe(sourceMaps.init())
        .pipe(less())
        .pipe(concat(config.cssBundle))
        .pipe(dest(config.cssDest))
        .pipe(minifyCss())
        .pipe(concat(config.cssBundleMin))
        .pipe(sourceMaps.write(config.maps))
        .pipe(dest(config.cssDest));
};

const typescript = function (callBack) {
    const tsResult = tsProject.src()
        .pipe(tsProject());

    return tsResult.js.pipe(dest(paths.source));
};

const cleanScriptBundles = function (callBack) {
    return del([
        config.script(config.libraryBundle),
        config.script(config.angularBundle),
        config.script(config.appBundle),
        config.script(config.appReportBundle),
    ]);
};

const cleanScripts = parallel(cleanScriptBundles, cleanJsAndMaps);

// create external library bundled file
const libraryBundle = function (callBack) {
    return src(config.librarySrc)
        .pipe(sourceMaps.init())
        .pipe(concat(config.libraryBundle))
        .pipe(sourceMaps.write(config.maps))
        .pipe(dest(paths.sourceScript));
};

// create angular library bundled file
const angularBundle = function (callBack) {
    return src(config.angularSrc)
        .pipe(sourceMaps.init())
        .pipe(concat(config.angularBundle))
        .pipe(sourceMaps.write(config.maps))
        .pipe(dest(paths.sourceScript));
};

// create app bundled file
const appBundle = function (callBack) {
    return src(config.appSrc)
        .pipe(sourceMaps.init())
        .pipe(concat(config.appBundle))
        .pipe(uglify())
        .pipe(sourceMaps.write(config.maps))
        .pipe(dest(paths.sourceScript));
};

// create app report bundled file
const appReportBundle = function (callBack) {
    return src(config.appReportSrc)
        .pipe(sourceMaps.init())
        .pipe(concat(config.appReportBundle))
        .pipe(uglify())
        .pipe(sourceMaps.write(config.maps))
        .pipe(dest(paths.sourceScript));
};

// delete style output folders
const cleanStyles = function (callBack) {
    return del([
        config.fontDest,
        config.cssDest,
        config.cssDest + "/" + config.maps
    ]);
};

// copy assets
const assets = function (callBack) {
    return src(config.assetsSrc)
        .pipe(dest(config.cssDest));
};

// copy fonts
const fonts = function (callBack) {
    return src(config.fontSrc)
        .pipe(dest(config.fontDest));
};

// copy tinyMCE fonts
const tinyMCEFonts = function (callBack) {
    return src(config.tinyMCEFontSrc)
        .pipe(dest(config.tinyMCEFontDest));
};

const tinyMCEFixCss = function (callBack) {
    return file("content.min.css", "//Dummy file from gulp", { src: true })
    .pipe(dest(paths.sourceScript + "/skins/lightgray"))
    .pipe(rename("skin.min.css"))
    .pipe(dest(paths.sourceScript + "/skins/lightgray"));
};

const tinyMCEFixLang = function (callBack) {
    return file("da.js", "//Dummy file from gulp", { src: true })
    .pipe(dest(paths.sourceScript + "/langs"));
};

// bundle, minify and copy styles, fonts and assets
const styles = series(cleanStyles, parallel(css, assets, fonts), parallel(tinyMCEFonts, tinyMCEFixCss, tinyMCEFixLang));

// run bundle tasks
const scripts = series(cleanScriptBundles, parallel(appBundle, libraryBundle, angularBundle, appReportBundle));

// bundle and deploy scripts and styles
exports.deployProd = series(typescript, parallel(scripts, styles), cleanJsAndMaps);

exports.clean = parallel(cleanStyles, cleanScripts);