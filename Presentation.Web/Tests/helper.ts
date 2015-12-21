import mock = require("protractor-http-mock");

export class Browser {
    browser: protractor.IBrowser;

    constructor(browser: protractor.IBrowser) {
        this.browser = browser;
    }

    outputLog() {
        this.browser
            .manage()
            .logs()
            .get("browser")
            .then(browserLogs => {
                if (browserLogs && browserLogs.length > 0) {
                    console.log("\n*** Browser console output ***");

                    browserLogs.forEach(log => {
                        if (log.level.value > 900) {
                            console.log(log.message);
                        }
                    });

                    console.log("\n");
                }
            });
    }
}

export class Mock {
    constructor() { }

    /**
     * return last matched request
     */
    lastRequest(): webdriver.promise.Promise<mock.ReceivedRequest> {
        var promise = mock.requestsMade()
            .then((requests: Array<mock.ReceivedRequest>) => {
                var lastRequest = requests[requests.length - 1];

                if (!lastRequest) throw Error("protractor-http-mock: No requests matched with mocks.");

            return lastRequest;
        });

        return promise;
    }

    /**
     * output all matched requests to console
     */
    outputRequests(): webdriver.promise.Promise<void> {
        var promise = mock.requestsMade().then((requests: Array<mock.ReceivedRequest>) => {
            console.log("\n*** protractor-http-mock matched requests ***\n");

            for (var i = 0; i < requests.length; i++) {
                console.log(requests[i]);
            }

            console.log("\n");
        });

        return promise;
    }
}
