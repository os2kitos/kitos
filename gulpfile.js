"use strict";

const { watch, parallel } = require("gulp");
const paths = require("./paths.config.js");
const del = require("del");

// require gulp tasks from all gulp files
require("require-dir")("./gulp");

// watch for file changes and run linters.
exports.watch =  function() {
    watch(paths.allTypeScript, ["ts-lint"]);
    watch(paths.allJavaScript, ["es-lint"]);
};

// clean solution
exports.clean = parallel(del(paths.tempFiles));

