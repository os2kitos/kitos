beforeEach(() => {
    jasmine.addMatchers({
        "select2ToBeDisabled": (util: jasmine.MatchersUtil): jasmine.CustomMatcher => {
            // create error message from id or use the full HTML element if id is abcent
            var createErrorMessage = (actual, result) => {
                actual.getAttribute("id")
                    .then(id => {
                        if (!id) throw Error();
                        result.message = util.buildFailureMessage("select2ToBeDisabled", false, "#" + id.toString());
                    })
                    .then(null, () => {
                        actual.getOuterHtml()
                            .then(html => {
                                result.message = util.buildFailureMessage("select2ToBeDisabled", false, html.toString());
                            });
                    });
            }

            var compare = (actual) => {
                var result = {
                    pass: null,
                    message: null
                };

                result.pass = browser.wait(() => actual.getAttribute("class"), 2000)
                    .then((value: string) => {
                        // used if evaluation returns false
                        createErrorMessage(actual, result);
                        return value.search(".select2-container-disabled") !== -1;
                    }, () => {
                        // used if no class tag found or timeout
                        createErrorMessage(actual, result);
                        return false;
                    });

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
         * returns true if the select2 select box is disabled
         */
        select2ToBeDisabled(): boolean;
    }
}
