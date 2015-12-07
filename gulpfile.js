var gulp = require('gulp'),
    paths = require('./paths.config.js');

// require gulp tasks from all gulp files
require('require-dir')('./gulp');

// watch for file changes and run linters.
gulp.task('watch', function () {
    gulp.watch(paths.allTypeScript, ['ts-lint']);
    gulp.watch(paths.allJavaScript, ['es-lint']);
});

// run protractor tests
gulp.task('protractor', function () {
    var protractor = require('gulp-protractor').protractor;
    var browserstack = require('gulp-browserstack');

    var taskExitValue = 0;

    return gulp.src(paths.e2eFiles)
        .pipe(browserstack.startTunnel({
            key: process.env.BROWSERSTACK_KEY
        }))
        .pipe(protractor({
            configFile: 'protractor.conf.js'
        }))
        .on('error', function() {
            taskExitValue = 1;
            this.emit('end');
        })
        .pipe(browserstack.stopTunnel())
        .once('end', function () {
            // fix error where gulp is hanging after finish
            process.exit(taskExitValue);
        });
});
