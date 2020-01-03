/// <binding BeforeBuild='deployProd' />

const { watch } = require("gulp");
const paths = require("./paths.config.js");

// require gulp tasks from all gulp files
require("require-dir")("./gulp");

const deploy = require("./gulp/deploy.js");


// watch for file changes and run linters.
exports.watch =  function(cb) {
    watch(paths.allTypeScript, ["ts-lint"]);
    watch(paths.allJavaScript, ["es-lint"]);
    cb();
};

exports.deployProd = deploy.deployProd;

exports.deploy = deploy.deploy;