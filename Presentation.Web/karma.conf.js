module.exports = function (config) {
    config.set({
        // base path that will be used to resolve all patterns (eg. files, exclude)
        basePath: '',

        // frameworks to use
        // available frameworks: https://npmjs.org/browse/keyword/karma-adapter
        frameworks: ['jasmine'],

        // list of files / patterns to load in the browser
        files: [
            'Scripts/lodash.js',
            'Scripts/jquery-2.1.4.js',
            'Scripts/select2.js',
            'Scripts/moment.js',
            'Scripts/bootstrap.js',
            'Scripts/angular.js',
            'Scripts/i18n/angular-locale_da-dk.js',
            'Scripts/angular-animate.js',
            'Scripts/angular-sanitize.js',
            'Scripts/angular-ui-router.js',
            'Scripts/angular-ui/ui-bootstrap.js',
            'Scripts/angular-ui/ui-bootstrap-tpls.js',
            'Scripts/ui-select2.js',
            'Scripts/notify/*.js',
            'Scripts/angular-ui-util/ui-utils.js',
            'Scripts/app/**/*.js'
        ],

        browsers: ['Chrome'], // 'IE', 'Firefox',

        // list of files to exclude
        exclude: [
        ],

        // preprocess matching files before serving them to the browser
        // available preprocessors: https://npmjs.org/browse/keyword/karma-preprocessor
        preprocessors: {
            // source files, that you wanna generate coverage for
            // do not include tests or libraries
            // (these files will be instrumented by Istanbul)
            'Scripts/app/**/!(*.spec).js': ['coverage']
        },

        coverageReporter: {
            type : 'lcov',
            dir : 'coverage/'
        },

        // test results reporter to use
        // possible values: 'dots', 'progress'
        // available reporters: https://npmjs.org/browse/keyword/karma-reporter
        reporters: ['progress', 'coverage', 'coveralls'],

        // web server port
        port: 9876,

        // enable / disable colors in the output (reporters and logs)
        colors: true,

        // level of logging
        // possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
        logLevel: config.LOG_INFO,

        // enable / disable watching file and executing tests whenever any file changes
        autoWatch: true,

        // Continuous Integration mode
        // if true, Karma captures browsers, runs the tests and exits
        singleRun: false,

        // Concurrency level
        // how many browser should be started simultanous
        concurrency: Infinity
    });
}
