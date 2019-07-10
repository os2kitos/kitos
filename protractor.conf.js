'use strict';

var paths = require('./paths.config.js');

exports.config = {
    framework: 'jasmine2',

    seleniumAddress: 'http://localhost:4444/wd/hub',

    capabilities: {
        browserName: 'chrome',
        shardTestFiles: true,
        maxInstances: 1
    },



    //specs: ['Presentation.Web/Test/it-contract/it-contract-edit.e2e.spec.js'],

    baseUrl: 'https://localhost:44300',

    onPrepare: function () {

        require("jasmine-expect");

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
    },

    params: {
        login: {
            email: 'default@kitos.dk',
            pwd: 'default'
        }
    }
};
