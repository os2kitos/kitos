beforeEach(() => {
    jasmine.addMatchers({
        "toBeDisabled": (util: jasmine.MatchersUtil): jasmine.CustomMatcher => {

            var compare = (actual) => {

                //var message = util.buildFailureMessage("toBeDisabled", false, actual.getId());
                var result = {
                    pass: null,
                    message: null
                };

                result.pass = browser.wait(() => actual.getAttribute("disabled"), 2000)
                    .then(value => {
                        return util.equals(value, "true");
                    }, () => {
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
                                        actual.getOuterHtml()
                                            .then(html => {
                                                result.message = util.buildFailureMessage("toBeDisabled", false, html.toString());
                                            });
                                    });
                            });
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
