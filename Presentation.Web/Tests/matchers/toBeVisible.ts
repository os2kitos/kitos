beforeEach(() => {
    jasmine.addMatchers({
        "toBeVisible": (util: jasmine.MatchersUtil): jasmine.CustomMatcher => {
            var compare = (actual, expected) => {
                var result = {
                    pass: null,
                    message: null
                };

                // create error message from id, name, data-ng-model or use the full HTML element if others are abcent
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
                        .then(value => result.message = util.buildFailureMessage("toBeVisible", negated, value));
                };

                if (!actual.isDisplayed) {
                    getElementIdentifier().then(v => {
                        throw Error("Can't determine visibility of '" + v + "'. Method isDisplayed() is not defined.");
                    });
                }

                result.pass = browser.wait(actual.isDisplayed(), 2000)
                    .then((v: boolean) => {
                        setErrorMessage(v); // negated on pass in case .not is used on matcher

                        return v;
                    }, () => {
                        getElementIdentifier().then(v => {
                            throw Error("Can't determine visibility of '" + v + "'. Method isDisplayed() timed out.");
                        });
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
         * returns true if the element is visible in the DOM
         */
        toBeVisible(): boolean;
    }
}
