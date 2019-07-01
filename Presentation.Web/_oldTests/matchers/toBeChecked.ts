beforeEach(() => {
    jasmine.addMatchers({
        "toBeChecked": (util: jasmine.MatchersUtil): jasmine.CustomMatcher => {
            var compare = (actual) => {
                var result = {
                    pass: null,
                    message: null
                };

                if (!actual.isSelected) {
                    throw Error("Can't determine if element is cheked. Method isSelected() is not defined. Are you expecting on a 'protractor.ElementArrayFinder'?");
                }

                result.pass = actual.isSelected().then(v => {
                        setErrorMessage(v);
                        return v;
                    });

                return result;

                // get element identifier from id, name, data-ng-model or use the full HTML element if others are abcent
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
                }

                function setErrorMessage(negated: boolean = false) {
                    getElementIdentifier()
                        .then(value => result.message = util.buildFailureMessage("toBeChecked", negated, value));
                };
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
         * returns true if the element is checked
         */
        toBeChecked(): boolean;
    }
}
