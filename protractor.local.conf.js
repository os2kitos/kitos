'use strict';

var paths = require('./paths.config.js');

exports.config = {
    framework: 'jasmine2',

    seleniumAddress: 'http://localhost:4444/wd/hub',

    capabilities: {
        browserName: 'chrome',
        shardTestFiles: true,
        maxInstances: 1
        /* PhantomJS is not working
         * Can be used to specify the phantomjs binary path.
         * This can generally be ommitted if you installed phantomjs globally.
         */
        //'phantomjs.binary.path': './node_modules/phantomjs-prebuilt/lib/phantom/bin/phantomjs',
        /*
         * Command line args to pass to ghostdriver, phantomjs's browser driver.
         * See https://github.com/detro/ghostdriver#faq
         */
        //'phantomjs.ghostdriver.cli.args': ['--loglevel=DEBUG']
    },

    // kan ikke bruge suites når vi tester vha. gulp
    // select all end to end tests
    //suites: paths.e2eSuites.home,
    //suites:[],

    //specs: ['Presentation.Web/Test/it-contract/it-contract-edit.e2e.spec.js'],

    baseUrl: 'https://localhost:44300',

    onPrepare: function () {

        require("jasmine-expect");
        require("require-dir")("./Presentation.Web/Tests/matchers");

        var reporters = require("jasmine-reporters");
        jasmine.getEnv()
            .addReporter(new reporters.TerminalReporter({
                verbosity: 3,
                color: true,
                showStack: true
            }));

        var HtmlReporter = require("protractor-beautiful-reporter");
        jasmine.getEnv().addReporter(new HtmlReporter({
            baseDirectory: 'tmp/screenshots'
        }).getJasmine2Reporter());
    },

    jasmineNodeOpts: {
        print: function () { },
        showColors: true,
        defaultTimeoutInterval: 30000
    },

    mocks: {
        default: ['authorize'],
        dir: paths.source + '/Tests/mocks'
    }
};
