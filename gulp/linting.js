"use strict";

const { src, series } = require("gulp");
const paths = require("../paths.config.js");


// run tslint on all typescript files.
const tsLint = function (callBack) {
    const tslint = require("gulp-tslint");

    return src("Presentation.Web/app**/*.ts")
		.pipe(tslint())
		.pipe(tslint.report());
};

// run eslint on all javascript files
const esLint = function (callBack) {
    const eslint = require("gulp-eslint");

    return src(paths.allJavaScript)
		.pipe(eslint())
		.pipe(eslint.format());
};


exports.lint = series(tsLint, esLint);