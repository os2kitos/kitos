import mock = require("protractor-http-mock");

export class Browser {
    constructor(private browser: protractor.IBrowser) { }

    /**
     * output browser error log
     */
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

    /**
     * dismiss open alert box
     */
    dismissAlert(): webdriver.promise.Promise<void> {
        return browser.switchTo().alert()
            .then(alert => alert.dismiss());
    }

    /**
     * accept open alert box
     */
    acceptAlert(): webdriver.promise.Promise<void> {
        return browser.switchTo().alert()
            .then(alert => alert.accept());
    }
}

export class Mock {
    /**
     * return last matched request
     */
    lastRequest(): webdriver.promise.Promise<mock.ReceivedRequest> {
        var promise = mock.requestsMade()
            .then((requests: Array<mock.ReceivedRequest>) => {
                var lastRequest = requests[requests.length - 1];

                if (!lastRequest) {
                    throw Error("protractor-http-mock: No requests matched with mocks.");
                }

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
            if (requests.length === 0) {
                console.log("    No requests matched with mocks.");
            }

            for (var i = 0; i < requests.length; i++) {
                console.log(`${i + 1}\n  METHOD: ${requests[i].method}\n  URL   : ${requests[i].url}`);

                if (requests[i].data) {
                    console.log("  DATA  : ", requests[i].data);
                }
            }

            console.log("\n");
        });

        return promise;
    }
}
