"use strict";

const { src, series } = require("gulp");
const paths = require("../paths.config.js");


// run tslint on all typescript files.
const tsLint = function() {
    const tslint = require("gulp-tslint");

    return src("Presentation.Web/app**/*.ts")
		.pipe(tslint())
		.pipe(tslint.report());
};

// run eslint on all javascript files
const esLint = function() {
    const eslint = require("gulp-eslint");

    return src(paths.allJavaScript)
		.pipe(eslint())
		.pipe(eslint.format());
    // Use this to fail build on errors
    //.pipe(eslint.failAfterError());
};


exports.lint = series(tsLint, esLint);