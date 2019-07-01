beforeEach(function () {
    jasmine.addMatchers({
        "toMatchInRequests": function (util) {
            var compare = function (actual, expected) {
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
                result.message = util.buildFailureMessage("toMatchInRequests", result.pass, outputRequests(actual), "\n  " + outputRequest(expected));
                return result;
            };
            // output a single request
            function outputRequest(request) {
                return "METHOD: " + request.method + " URL: " + request.url;
            }
            ;
            // output all requests
            function outputRequests(requests) {
                var output = "Actual requests: " + requests.length;
                if (requests.length > 0) {
                    output += "\n";
                    for (var i = 0; i < requests.length; i++) {
                        output += "  " + outputRequest(requests[i]);
                        if (i < requests.length - 1)
                            output += "\n";
                    }
                    output += "\n";
                }
                return output;
            }
            ;
            // compare two requests
            function compareRequest(actual, expected) {
                return actual.method === expected.method && actual.url.search(expected.url) !== -1;
            }
            ;
            return {
                compare: compare
            };
        }
    });
});
