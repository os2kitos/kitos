beforeEach(() => {
    jasmine.addMatchers({
        "toHaveClass": (util: jasmine.MatchersUtil): jasmine.CustomMatcher => {
            var compare = (actual, expected) => {
                var result = {
                    pass: null,
                    message: null
                };

                if (!actual.getAttribute) {
                    throw Error("Can't determine if element has class. Method getAttribute() is not defined. Are you expecting on a 'protractor.ElementArrayFinder'?");
                }

                result.pass = browser.wait(actual.getAttribute("class"), 2000)
                    .then((v: string) => {
                        // find exact case-insensitive class
                        var re = new RegExp("(\w+)?(" + expected + ")(?!-)(\w+)?", "i");
                        var result = v.search(re) !== -1;

                        setErrorMessage(result, v); // negated on pass in case .not is used on matcher

                        return result;
                    }, () => {
                        getElementIdentifier().then(v => {
                            throw Error("Can't determine if '" + v + "' has class. Method getAttribute() timed out.");
                        });
                    });

                return result;

                // create error message from id, name, data-ng-model or use the full HTML element if others are abcent
                function getElementIdentifier(): webdriver.promise.Promise<string> {
                    if (!actual.getAttribute) {
                        throw Error("Can't determine identifier for element. Method getAttribute() is undefined.");
                    }

                    return actual.getAttribute("id")
                        .then(id => {
                            if (!id) throw Error();
                            return `#${id.toString()}`;
                        })
                        .thenCatch(() => {
                            return actual.getAttribute("data-ng-model")
                                .then(model => {
                                    if (!model) throw Error();
                                    return model.toString();
                                });
                        })
                        .thenCatch(() => {
                            return actual.getAttribute("name")
                                .then(name => {
                                    if (!name) throw Error();
                                    return name.toString();
                                });
                        })
                        .thenCatch(() => {
                            return actual.getOuterHtml()
                                .then(html => {
                                    return html.toString();
                                });
                        });
                }

                function setErrorMessage(negated: boolean = false, message: string = null) {
                    getElementIdentifier()
                        .then(value => result.message = util.buildFailureMessage("toHaveClass", negated, value + (message ? `: "${message}"` : ""), expected));
                }
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
         * returns true if the element has class
         */
        toHaveClass(value: string): boolean;
    }
}
