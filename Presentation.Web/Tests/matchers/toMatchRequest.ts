beforeEach(() => {
    jasmine.addMatchers({
        "toMatchRequest": (util: jasmine.MatchersUtil): jasmine.CustomMatcher => {
            var outputRequest = (request: mock.ReceivedRequest): string => {
                return `METHOD: ${request.method} URL: ${request.url}`;
            };

            var compare = (actual, expected) => {
                var result = {
                    pass: null,
                    message: null
                };

                result.pass = actual.method === expected.method && actual.url.search(expected.url) !== -1;

                result.message = util.buildFailureMessage("toMatchRequest", result.pass, outputRequest(actual), outputRequest(expected));

                return result;
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
         * returns true if the last mocked request is as expected
         */
        toMatchRequest(expected: mock.ReceivedRequest): boolean;
    }
}
