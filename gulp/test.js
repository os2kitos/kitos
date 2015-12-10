var gulp = require('gulp'),
    paths = require('../paths.config.js'),
    karma = require('karma');

// run karma tests and coverage
gulp.task('unit', function () {
    new karma.Server({
        configFile: paths.karmaConf,
        singleRun: true,
        browsers: ['IE', 'Firefox', 'Chrome'],
        reporters: ['progress', 'coverage'],
        coverageReporter: {
            dir: paths.coverage,
            reporters: [
                { type: 'cobertura', subdir: '.', file: 'frontend.xml' }
            ]
        },
        preprocessors: {
            // source files, that you wanna generate coverage for
            // do not include tests or libraries
            // (these files will be instrumented by Istanbul)
            'Presentation.Web/app/**/!(*.spec|*.po).js': ['coverage']
        },
        autoWatch: false
    }).start();
});

// open coverage results.
gulp.task('localCover', ['localUnit'], function () {
    var open = require('gulp-open');

    gulp.src(paths.coverage + '/Phantom*/index.html')
        .pipe(open({
            app: 'chrome'
        }));
});

// run karma tests and coverage locally
gulp.task('localUnit', function (done) {
    new karma.Server({
        configFile: paths.karmaConf,
        singleRun: true,
        reporters: ['progress', 'coverage'],
        coverageReporter: {
            type: 'html',
            dir: paths.coverage
        },
        preprocessors: {
            // source files, that you wanna generate coverage for
            // do not include tests or libraries
            // (these files will be instrumented by Istanbul)
            'Presentation.Web/app/**/!(*.spec|*.po).js': ['coverage']
        },
        autoWatch: false
    }, done).start();
});

// run e2e tests with protractor
gulp.task('e2e', function () {
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
        .on('error', function () {
            taskExitValue = 1;
            this.emit('end');
        })
        .pipe(browserstack.stopTunnel())
        .once('end', function () {
            // fix error where gulp is hanging after finish
            process.exit(taskExitValue);
        });
});

// publish coverage to codecov
gulp.task('codecov', function() {
    var codecov = require('gulp-codecov.io');

    return gulp.src('./coverage/*.xml')
        .pipe(codecov());
});
