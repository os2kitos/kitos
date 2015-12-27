beforeEach(() => {
    jasmine.addMatchers({
        "toBeDisabled": (util: jasmine.MatchersUtil): jasmine.CustomMatcher => {
            var compare = (actual) => {
                var result = {
                    pass: null,
                    message: null
                };

                // get element identifier from id, name, data-ng-model or use the full HTML element if others are abcent
                var getElementIdentifier = (): webdriver.promise.Promise<string> => {
                    return actual.getAttribute("id")
                        .then(id => {
                            if (!id) throw Error();
                            return "#" + id.toString();
                        })
                        .thenCatch(() => {
                            return actual.getAttribute("name")
                                .then(name => {
                                    if (!name) throw Error();
                                    return name.toString();
                                });
                        })
                        .thenCatch(() => {
                            return actual.getAttribute("data-ng-model")
                                .then(model => {
                                    if (!model) throw Error();
                                    return model.toString();
                                });
                        })
                        .thenCatch(() => {
                            return actual.getOuterHtml()
                                .then(html => {
                                    return html.toString();
                                });
                        });
                };

                var setErrorMessage = (negated: boolean = false) => {
                    getElementIdentifier()
                        .then(value => result.message = util.buildFailureMessage("toBeDisabled", negated, value));
                };

                if (!actual.getAttribute) {
                    getElementIdentifier().then(v => {
                        throw Error("Can't determine if '" + v + "' is disabled. Method getAttribute() is not defined.");
                    });
                }

                // protractor always returns true if disabled attribute is pressent
                // otherwise it rejects the promise
                result.pass = browser.wait(() => actual.getAttribute("disabled"), 2000)
                    .then(() => {
                        setErrorMessage(true); // used when .not is used on matcher
                        return true;
                    }, () => {
                        setErrorMessage();
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
