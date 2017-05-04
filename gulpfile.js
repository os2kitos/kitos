/// <binding BeforeBuild='deploy-prod' />
var gulp = require('gulp'),
    paths = require('./paths.config.js');

// require gulp tasks from all gulp files
require('require-dir')('./gulp');

// watch for file changes and run linters.
gulp.task('watch', function () {
    gulp.watch(paths.allTypeScript, ['ts-lint']);
    gulp.watch(paths.allJavaScript, ['es-lint']);
});

// clean solution
gulp.task('clean', ["clean-styles", "clean-scripts"], function() {
    var del = require('del');

    return del(paths.tempFiles);
});
