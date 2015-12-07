var gulp = require('gulp'),
    paths = require('../paths.config.js'),
    karma = require('karma');

// run karma tests and coverage. Post coverage to Coveralls.io
gulp.task('karmaPost', function () {
    new karma.Server({
        configFile: __dirname + '/' + paths.source + '/karma.conf.js',
        singleRun: true,
        browsers: ['IE', 'Firefox', 'Chrome'],
        reporters: ['progress', 'coverage', 'coveralls'],
        coverageReporter: {
            type: 'lcov',
            dir: 'karmaCoverage/'
        },
        preprocessors: {
            // source files, that you wanna generate coverage for
            // do not include tests or libraries
            // (these files will be instrumented by Istanbul)
            '../Presentation.Web/Scripts/app/**/!(*.spec).js': ['coverage']
        },
        autoWatch: false
    }).start();
});

// run karma tests and coverage. No publish to Coveralls.io
gulp.task('karma', function () {
    new karma.Server({
        configFile: __dirname + '/' + paths.source + '/karma.conf.js',
        singleRun: true,
        browsers: ['IE', 'Firefox', 'Chrome'],
        reporters: ['progress', 'coverage'],
        coverageReporter: {
            type: 'html',
            dir: 'karmaCoverage/'
        },
        preprocessors: {
            // source files, that you wanna generate coverage for
            // do not include tests or libraries
            // (these files will be instrumented by Istanbul)
            '../Presentation.Web/Scripts/app/**/!(*.spec).js': ['coverage']
        },
        autoWatch: false
    }).start();
});

// open coverage results.
gulp.task('localCover', ['localKarma'], function () {
    var open = require('gulp-open');

    gulp.src('karmaCoverage/Phantom*/index.html')
        .pipe(open({
            app: 'chrome'
        }));
});

// run karma tests and coverage locally
gulp.task('localKarma', function (done) {
    new karma.Server({
        configFile: __dirname + '/../' + paths.source + '/karma.conf.js',
        singleRun: true,
        reporters: ['progress', 'coverage'],
        coverageReporter: {
            type: 'html',
            dir: 'karmaCoverage/'
        },
        preprocessors: {
            // source files, that you wanna generate coverage for
            // do not include tests or libraries
            // (these files will be instrumented by Istanbul)
            '../Presentation.Web/Scripts/app/**/!(*.spec).js': ['coverage']
        },
        autoWatch: false
    }, done).start();
});
