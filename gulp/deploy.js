var gulp = require("gulp");
var concat = require("gulp-concat");
var del = require("del");
var minifyCSS = require("gulp-minify-css");
var bower = require("gulp-bower");
var paths = require("../paths.config.js");
var config = require("../bundle.config.js");

// delete style output folders
gulp.task("clean-styles", function () {
    return del([
        config.fontDest,
        config.cssDest
    ]);
});

// copy assets
gulp.task("assets", ["clean-styles", "bower-restore"], function () {
    return gulp.src(config.assetsSrc)
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
gulp.task("styles", ["assets", "fonts"]);

// bundle and deploy scripts and styles
gulp.task("deploy", ["styles"]);
