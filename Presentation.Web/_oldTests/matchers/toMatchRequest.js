beforeEach(function () {
    jasmine.addMatchers({
        "toMatchRequest": function (util) {
            var outputRequest = function (request) {
                return "METHOD: " + request.method + " URL: " + request.url;
            };
            var compare = function (actual, expected) {
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
