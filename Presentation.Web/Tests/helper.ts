import mock = require('protractor-http-mock');

export class Browser {
    browser: protractor.IBrowser;

    constructor(browser: protractor.IBrowser) {
        this.browser = browser;
    }

    outputLog() {
        this.browser
            .manage()
            .logs()
            .get('browser')
            .then(browserLogs => {
                if (browserLogs && browserLogs.length > 0) {
                    console.log('\n*** Browser console output ***');

                    browserLogs.forEach(log => {
                        if (log.level.value > 900) {
                            console.log(log.message);
                        }
                    });

                    console.log('\n');
                }
            });
    }
}

export class Mock {
    constructor() { }

    private compareRequest(actual: mock.ReceivedRequest, expected: mock.ReceivedRequest): boolean {
        return actual.method === expected.method && actual.url.search(expected.url) !== -1;
    }

    lastRequest(expected: mock.ReceivedRequest): webdriver.promise.Promise<boolean> {
        var promise = mock.requestsMade()
            .then((requests: Array<mock.ReceivedRequest>) => {
                var lastRequest = requests[requests.length - 1];

                return this.compareRequest(lastRequest, expected);
        });

        return promise;
    }

    findRequest(expected: mock.ReceivedRequest): webdriver.promise.Promise<boolean> {
        var promise = mock.requestsMade().then((requests: Array<mock.ReceivedRequest>) => {
            for (var i = 0; i < requests.length; i++) {
                if (this.compareRequest(requests[i], expected)) {
                    return true;
                }
            }

            return false;
        });

        return promise;
    }

    outputRequests(): webdriver.promise.Promise<void> {
        var promise = mock.requestsMade().then((requests: Array<mock.ReceivedRequest>) => {
            console.log('\nprotractor-http-mock matched requests');

            for (var i = 0; i < requests.length; i++) {
                console.log(requests[i]);
            }

            console.log('\n');
        });

        return promise;
    }
}
