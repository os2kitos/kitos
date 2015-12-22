beforeEach(() => {
    jasmine.addMatchers({
        "toBeVisible": (util: jasmine.MatchersUtil): jasmine.CustomMatcher => {
            console.log(this);        
            var compare = (actual, expected) => {
                var result = {
                    pass: null,
                    message: null
                };

                // create error message from id, name, data-ng-model or use the full HTML element if others are abcent
                var createErrorMessage = () => {
                    actual.getAttribute("id")
                        .then(id => {
                            if (!id) throw Error();
                            setErrorMessage("#" + id.toString());
                        })
                        .then(null, () => {
                            actual.getAttribute("name")
                                .then(name => {
                                    if (!name) throw Error();
                                    setErrorMessage(name.toString());
                                })
                                .then(null, () => {
                                    actual.getAttribute("data-ng-model")
                                        .then(model => {
                                            if (!model) throw Error();
                                            setErrorMessage(model.toString());
                                        })
                                        .then(null, () => {
                                            actual.getOuterHtml()
                                                .then(html => {
                                                    setErrorMessage(html.toString());
                                                });
                                        });
                                });
                        });
                };
                var setErrorMessage = (value: string) => {
                    result.message = util.buildFailureMessage("toBeVisible", false, value);
                }

                createErrorMessage();

                result.pass = actual.isDisplayed();

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
