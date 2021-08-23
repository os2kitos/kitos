"use strict";

exports.config = {
    framework: "jasmine2",

    seleniumAddress: "http://localhost:4444/wd/hub",

    capabilities: {
        browserName: "chrome",
        shardTestFiles: true,
        maxInstances: 1, 
        chromeOptions: {
            args: ["--headless", "--disable-gpu"]
        }
    },

    baseUrl: "https://localhost:44300",

    onPrepare: function () {

        require("jasmine-expect");

        var reporters = require("jasmine-reporters");
        jasmine.getEnv()
            .addReporter(new reporters.TerminalReporter({
                verbosity: 3,
                color: true,
                showStack: true
            }));

        jasmine.getEnv().addReporter(new reporters.TeamCityReporter());
    },

    jasmineNodeOpts: {
        print: function () { },
        showColors: true,
        defaultTimeoutInterval: 30000
    }

};
