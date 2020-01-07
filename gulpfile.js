"use strict";

const { watch } = require("gulp");
const paths = require("./paths.config.js");

// require gulp tasks from all gulp files
const linting = require("./gulp/linting.js");
const test = require("./gulp/ui-test.js");
const deploy = require("./gulp/deploy.js");


// watch for file changes and run linters.
exports.watch = function (callBack) {
    watch(paths.allTypeScript, ["ts-lint"]);
    watch(paths.allJavaScript, ["es-lint"]);
    callBack();
};

exports.deployProd = deploy.deployProd;

exports.clean = deploy.clean;

exports.lint = linting.lint;

exports.e2eHeadless = test.runProtractorHeadless;
exports.e2eLocal = test.runProtractorLocal;
exports.e2eSingle = test.runProtractorSingle;