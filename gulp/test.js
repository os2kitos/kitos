var gulp = require('gulp'),
    paths = require('../paths.config.js'),
    karma = require('karma');

// run unit tests with karma
gulp.task('unit', function (done) {
    new karma.Server({
        configFile: paths.karmaConf,
        singleRun: true,
        browsers: ['IE', 'Firefox', 'Chrome'],
        reporters: ['progress', 'coverage'],
        coverageReporter: {
            type: 'json',
            subdir: '.',
            file: paths.tempFrontendCoverageReport
        },
        preprocessors: {
            'Presentation.Web/app/**/!(*.spec|*.po).js': ['coverage']
        },
        autoWatch: false,

        // to avoid DISCONNECTED messages on CI
        browserDisconnectTimeout : 10000, // default 2000
        browserDisconnectTolerance : 1, // default 0
        browserNoActivityTimeout : 60000 //default 10000
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

// map karma coverage results from js to ts source
gulp.task('mapCoverage', function (done) {
    var exec = require('child_process').exec;
    var del = require('del');

    exec('node_modules\\.bin\\remap-istanbul -i ' + paths.coverage + '/' + paths.tempFrontendCoverageReport + ' -o ' + paths.coverage + '/' + paths.frontendCoverageReport, function(err, stdout, stderr) {
        console.log(stdout);
        console.log(stderr);

        del([paths.coverage + '/' + paths.tempFrontendCoverageReport]);
        done();
    });
});

// publish coverage to codecov
gulp.task('codecov', ['mapCoverage'], function () {
    var codecov = require('gulp-codecov.io');

    return gulp.src(paths.coverage + '/?(frontend.json|backend.xml)')
        .pipe(codecov());
});

// run local unit tests and coverage report generator
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
            'Presentation.Web/app/**/!(*.spec|*.po).js': ['coverage']
        },
        autoWatch: false
    }, done).start();
});

// open coverage results.
gulp.task('localCover', ['localUnit'], function () {
    var open = require('gulp-open');

    gulp.src(paths.coverage + '/Phantom*/index.html')
        .pipe(open({
            app: 'chrome'
        }));
});
