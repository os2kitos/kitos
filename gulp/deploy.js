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
        config.script(config.kendoBundle),
        config.script(config.appBundle),
        config.script(config.maps)
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
     .pipe(sourcemaps.write(config.maps))
     .pipe(gulp.dest(paths.sourceScript));
});

// create angular library bundled file
gulp.task("kendo-bundle", ["clean-scripts", "bower-restore"], function () {
    return gulp.src(config.kendoSrc)
     .pipe(concat(config.kendoBundle))
     .pipe(gulp.dest(paths.sourceScript));
});

gulp.task("kendo-css", function () {
    return gulp.src(config.kendoStylesSrc)
        .pipe(concat(config.kendoCss))
        .pipe(gulp.dest(config.kendo("")))
        .pipe(minifyCSS())
        .pipe(concat(config.kendoCssMin))
        .pipe(gulp.dest(config.kendo("")));
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
        config.kendo(config.kendoCss),
        config.kendo(config.kendoCssMin)
    ]);
});

// copy assets
gulp.task("assets", ["clean-styles", "bower-restore"], function () {
    return gulp.src(config.assetsSrc)
        .pipe(gulp.dest(config.cssDest));
});

// create css bundled file
gulp.task("css", ["clean-styles", "bower-restore"], function () {
    return gulp.src(config.libraryStylesSrc
            .concat(config.customCssSrc))
        .pipe(concat(config.cssBundle))
        .pipe(gulp.dest(config.cssDest))
        .pipe(minifyCSS())
        .pipe(concat(config.cssBundleMin))
        .pipe(gulp.dest(config.cssDest));
});

// copy fonts
gulp.task("fonts", ["clean-styles", "bower-restore"], function () {
    return gulp.src(config.fontSrc)
        .pipe(gulp.dest(config.fontDest));
});

// restore all bower packages
gulp.task("bower-restore", function () {
    return bower()
        .pipe(gulp.dest(paths.bowerComponents));
});

// bundle, minify and copy styles, fonts and assets
gulp.task("styles", ["css", "kendo-css", "assets", "fonts"]);

// run bundle tasks
gulp.task("scripts", ["library-bundle", "kendo-bundle", "angular-bundle"]);

// bundle and deploy scripts and styles
gulp.task("deploy", ["scripts", "styles"]);
