beforeEach(() => {
    jasmine.addMatchers({
        "toMatchInRequests": (util: jasmine.MatchersUtil): jasmine.CustomMatcher => {
            var compare = (actual, expected) => {
                var result = {
                    pass: false,
                    message: null
                };

                for (var i = 0; i < actual.length; i++) {
                    if (compareRequest(actual[i], expected)) {
                        result.pass = true;
                        break;
                    }
                }

                result.message = util.buildFailureMessage("toMatchInRequests", result.pass, outputRequests(actual), `\n  ${outputRequest(expected)}`);

                return result;
            };

            // output a single request
            function outputRequest(request: mock.ReceivedRequest) {
                return `METHOD: ${request.method} URL: ${request.url}`;
            };

            // output all requests
            function outputRequests(requests: Array<mock.ReceivedRequest>) {
                var output = `Actual requests: ${requests.length}`;
                if (requests.length > 0) {
                    output += "\n";
                    for (var i = 0; i < requests.length; i++) {
                        output += `  ${outputRequest(requests[i])}`;
                        if (i < requests.length - 1)
                            output += "\n";
                    }
                    output += "\n";
                }

                return output;
            };

            // compare two requests
            function compareRequest(actual: mock.ReceivedRequest, expected: mock.ReceivedRequest) {
                return actual.method === expected.method && actual.url.search(expected.url) !== -1;
            };

            return {
                compare: compare
            };
        }
    });
});

declare module jasmine {
    interface Matchers {
        /**
         * returns true if expected request is in mocked requests
         *
         * @param expected A mock.ReceivedRequest object. URL is matched with regex.
         */
        toMatchInRequests(expected: mock.ReceivedRequest): boolean;
    }
}
