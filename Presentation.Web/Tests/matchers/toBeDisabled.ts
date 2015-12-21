beforeEach(() => {
    jasmine.addMatchers({
        "toBeDisabled": (util: jasmine.MatchersUtil): jasmine.CustomMatcher => {
            // create error message from id, name, data-ng-model or use the full HTML element if others are abcent
            var createErrorMessage = (actual, result) => {
                actual.getAttribute("id")
                    .then(id => {
                        if (!id) throw Error();
                        result.message = util.buildFailureMessage("toBeDisabled", false, "#" + id.toString());
                    })
                    .then(null, () => {
                        actual.getAttribute("name")
                            .then(name => {
                                if (!name) throw Error();
                                result.message = util.buildFailureMessage("toBeDisabled", false, name.toString());
                            })
                            .then(null, () => {
                                actual.getAttribute("data-ng-model")
                                    .then(model => {
                                        if (!model) throw Error();
                                        result.message = util.buildFailureMessage("toBeDisabled", false, "ngModel " + model.toString());
                                    })
                                    .then(null, () => {
                                        actual.getOuterHtml()
                                            .then(html => {
                                                result.message = util.buildFailureMessage("toBeDisabled", false, html.toString());
                                            });
                                    });
                            });
                    });
            };

            var compare = (actual) => {

                var result = {
                    pass: null,
                    message: null
                };

                result.pass = browser.wait(() => actual.getAttribute("disabled"), 2000)
                    .then(value => {
                        createErrorMessage(actual, result);
                        return util.equals(value, "true");
                    }, () => {
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
         * returns true if the elements attribute "disabled" is true
         */
        toBeDisabled(): boolean;
    }
}
