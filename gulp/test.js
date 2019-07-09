'use strict';

var gulp = require('gulp');
var paths = require('../paths.config.js');
var gutil = require('gulp-util');
var protractor = require('gulp-protractor');
var karma = require('karma');
var browserSync = require('browser-sync');
var $ = require('gulp-load-plugins')();
var del = require("del");


// run unit tests with karma
gulp.task('unit', function (done) {
    new karma.Server({
        configFile: paths.karmaConf,
        singleRun: true,
        browsers: ['IE', 'Chrome'],
        reporters: ['progress', 'coverage', 'teamcity'],
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
        browserDisconnectTimeout: 10000, // default 2000
        browserDisconnectTolerance: 1, // default 0
        browserNoActivityTimeout: 60000 //default 10000
    }, done).start();
});

gulp.task("CleanProtractor", cleanProtractor);

function cleanProtractor() {
    return del("tmp");
}

// use protractor
gulp.task('e2e:headless', ['CleanProtractor'], runProtractorHeadless);

function runProtractorHeadless(done) {
    var params = process.argv;
    var args = params.length > 3 ? [params[3], params[4], params[5]] : [];

    gutil.log('e2e arguments: ' + args);

    var singleSpec = 'Presentation.Web/Tests/**/*.e2e.spec.js';
    gulp.src(singleSpec) // paths.e2eSuites.itSystem
        .pipe(protractor.protractor({
            configFile: 'protractor.headless.conf.js',
            args: [
                '--params.login.email', args[0],
                '--params.login.pwd', args[1],
                '--baseUrl', args[2]
            ],
            'debug': false
        }))
        .on('error', function (err) {
            gutil.log(gutil.colors.red('error: ' + err));
            // Make sure failed tests cause gulp to exit non-zero
            throw err;
        })
        .on('end', function () {
            // Close browser sync server
            browserSync.exit();
            done();
        });
}

gulp.task('e2e:local', ['CleanProtractor'], runProtractorLocal);

function runProtractorLocal(done) {
    var params = process.argv;
    var args = params.length > 3 ? [params[3], params[4], params[5]] : [];

    gutil.log('e2e arguments: ' + args);

    var singleSpec = 'Presentation.Web/Tests/**/*.e2e.spec.js';
    gulp.src(singleSpec) // paths.e2eSuites.itSystem
        .pipe(protractor.protractor({
            configFile: 'protractor.local.conf.js',
            args: [
                '--params.login.email', args[0],
                '--params.login.pwd', args[1],
                '--baseUrl', args[2]
            ],
            'debug': false
        }))
        .on('error', function (err) {
            gutil.log(gutil.colors.red('error: ' + err));
            // Make sure failed tests cause gulp to exit non-zero
            throw err;
        })
        .on('end', function () {
            // Close browser sync server
            browserSync.exit();
            done();
        });
}

gulp.task('e2e:single', ['CleanProtractor'], runSingleTest);

function runSingleTest(done) {
    var params = process.argv;
    var args = params.length > 3 ? [params[3], params[4], params[5]] : [];

    gutil.log('e2e arguments: ' + args);

    var singleSpec = ['Presentation.Web/Tests/HomePage/Home.e2e.spec.js'];
    gulp.src(singleSpec) // paths.e2eSuites.itSystem
        .pipe(protractor.protractor({
            configFile: 'protractor.local.conf.js',
            args: [
                '--params.login.email', args[0],
                '--params.login.pwd', args[1],
                '--baseUrl', args[2]
            ],
            'debug': false
        }))
        .on('error', function (err) {
            gutil.log(gutil.colors.red('error: ' + err));
            // Make sure failed tests cause gulp to exit non-zero
            throw err;
        })
        .on('end', function () {
            // Close browser sync server
            browserSync.exit();
            done();
        });
}


// map karma coverage results from js to ts source
gulp.task('mapCoverage', function (done) {
    var exec = require('child_process').exec;
    var del = require('del');

    exec('node_modules\\.bin\\remap-istanbul -i ' + paths.coverage + '/' + paths.tempFrontendCoverageReport + ' -o ' + paths.coverage + '/' + paths.frontendCoverageReport, function (err, stdout, stderr) {
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
