var gulp = require('gulp'),
    paths = require('../paths.config.js'),
    gutil = require('gulp-util'),
    protractor = require('gulp-protractor').protractor,
    karma = require('karma');

// run unit tests with karma
gulp.task('unit', function (done) {
    new karma.Server({
        configFile: paths.karmaConf,
        singleRun: true,
        browsers: ['IE', 'Firefox', 'Chrome'],
        reporters: ['progress', 'coverage', 'appveyor'],
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
    return gulp.src(paths.e2eFiles)
        .pipe(protractor({
            configFile: 'protractor.conf.js'
        }));
});

// start standalone selenium webdriver
gulp.task('webdriver', require('gulp-protractor').webdriver_standalone);

// run e2e tests with protractor locally
gulp.task('locale2e', function () {
    var taskExitValue = 0;
    var args = process.argv;
    if (args && args.length > 3) {
        args = args.slice(3, args.length);
    } else {
        args = null;
    }

    return gulp.src(paths.e2eFiles)
        .pipe(protractor({
            configFile: 'protractor.local.conf.js',
            args: args
        }))
        .on('error', function (err) {
            gutil.log(gutil.colors.red("An error occurred in protractor. Did you start the webdriver?"));
            gutil.log(gutil.colors.red("Run cmd 'start gulp webdriver'."));
        });
});

// map karma coverage results from js to ts source
gulp.task('mapCoverage', function (done) {
    var exec = require('child_process').exec;
    var del = require('del');

    exec('node_modules\\.bin\\remap-istanbul -i ' + paths.coverage + '/' + paths.tempFrontendCoverageReport + ' -o ' + paths.coverage + '/' + paths.frontendCoverageReport, function(err, stdout, stderr) {
        gutil.log(stdout);
        gutil.log(gutil.colors.red(stderr));

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
