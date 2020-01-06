"use strict";

const { src, parallel } = require("gulp");
const paths = require("../paths.config.js");


// run tslint on all typescript files.
const tsLint = function() {
    const tslint = require("gulp-tslint");

    const args = process.argv;
    var path = paths.allTypeScript;
    if (args && args.length > 3 && args[3] === "--path") {
        path = args[4];
    }

    return src(path)
		.pipe(tslint())
		.pipe(tslint.report("prose", {
		    emitError: false // Set to true to fail build on errors
		}));
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


exports.lint = parallel(tsLint, esLint);