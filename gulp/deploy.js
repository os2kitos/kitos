var gulp = require("gulp");
var concat = require("gulp-concat");
var uglify = require("gulp-uglify");
var del = require("del");
var minifyCSS = require("gulp-minify-css");
var bower = require("gulp-bower");
var sourcemaps = require("gulp-sourcemaps");
var paths = require("../paths.config.js");
var config = require("../bundle.config.js");

//Synchronously delete the output script file(s)
gulp.task("clean-scripts", function () {
    return del([
        config.script(config.libraryBundle),
        config.script(config.angularBundle),
        config.script(config.appBundle),
        paths.allJavaScriptNoTests,
        paths.appMaps
    ]);
});

// create stimulsoft report ressources
// gulp.task("stimulsoft-bundle", ["clean-scripts", "bower-restore"], function () {
//     return gulp.src(config.stimulsoftSrc)
//         .pipe(sourcemaps.init())
//         .pipe(concat(config.stimulsoftBundle))
//         .pipe(sourcemaps.write(config.maps))
//         .pipe(gulp.dest(paths.sourceScript));
// });

// create external library bundled file
gulp.task("library-bundle", ["clean-scripts"], function () {
    return gulp.src(config.librarySrc)
        .pipe(sourcemaps.init())
        .pipe(concat(config.libraryBundle))
        .pipe(sourcemaps.write(config.maps))
        .pipe(gulp.dest(paths.sourceScript));
});

// create angular library bundled file
gulp.task("angular-bundle", ["clean-scripts"], function () {
    return gulp.src(config.angularSrc)
        .pipe(sourcemaps.init())
        .pipe(concat(config.angularBundle))
        .pipe(sourcemaps.write(config.maps))
        .pipe(gulp.dest(paths.sourceScript));
});

// create app bundled file
// ASP.NET bundler used instead og this.
gulp.task("app-bundle", ["clean-scripts"], function () {
    return gulp.src(config.appSrc)
        .pipe(sourcemaps.init())
        .pipe(uglify())
        .pipe(concat(config.appBundle))
        .pipe(sourcemaps.write(config.maps))
        .pipe(gulp.dest(paths.sourceScript));
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

// create css bundled file
gulp.task("css", ["clean-styles"], function () {
    return gulp.src(config.libraryStylesSrc.concat(config.customCssSrc))
        .pipe(sourcemaps.init())
        .pipe(concat(config.cssBundle))
        .pipe(gulp.dest(config.cssDest))
        .pipe(minifyCSS())
        .pipe(concat(config.cssBundleMin))
        .pipe(sourcemaps.write(config.maps))
        .pipe(gulp.dest(config.cssDest));
});

// copy fonts
gulp.task("fonts", ["clean-styles"], function () {
    return gulp.src(config.fontSrc)
        .pipe(gulp.dest(config.fontDest));
});

// restore all bower packages
gulp.task("bower-restore", function () {
    return bower(paths.bowerComponents);
});

// bundle, minify and copy styles, fonts and assets
gulp.task("styles", ["css", "assets", "fonts"]);

// run bundle tasks
gulp.task("scripts", ["library-bundle", "angular-bundle"]);

// bundle and deploy scripts and styles
gulp.task("deploy", ["scripts", "styles"]);
