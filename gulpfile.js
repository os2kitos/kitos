"use strict";

const { watch } = require("gulp");
const paths = require("./paths.config.js");

// require gulp tasks from all gulp files
const linting = require("./gulp/linting.js");
const test = require("./gulp/test.js");
const deploy = require("./gulp/deploy.js");


// watch for file changes and run linters.
exports.watch =  function(cb) {
    watch(paths.allTypeScript, ["ts-lint"]);
    watch(paths.allJavaScript, ["es-lint"]);
    cb();
};

exports.deployProd = deploy.deployProd;

exports.deploy = deploy.deploy;

exports.clean = deploy.clean;

exports.lint = linting.lint;

exports.e2eHeadless = test.runProtractorHeadless;
exports.e2eLocal = test.runProtractorLocal;