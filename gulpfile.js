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
gulp.task('clean', function() {
    var clean = require('gulp-clean');

    return gulp.src(paths.tempFiles, { read: false })
        .pipe(clean());
});
