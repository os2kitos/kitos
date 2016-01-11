beforeEach(() => {
    jasmine.addMatchers({
        "toBeSelect2Disabled": (util: jasmine.MatchersUtil): jasmine.CustomMatcher => {
            var compare = (actual) => {
                var result = {
                    pass: null,
                    message: null
                };

                if (!actual.getAttribute) {
                    throw Error("Can't determine if element is disabled. Method getAttribute() is not defined. Are you expecting on a 'protractor.ElementArrayFinder'?");
                }

                result.pass = browser.wait(() => actual.getAttribute("class"), 2000)
                    .then((value: string) => {
                        var pass = value.search(".select2-container-disabled") !== -1;

                        setErrorMessage(pass); // negated on pass if .not is used on matcher

                        return pass;
                    }, () => {
                        getElementIdentifier()
                            .then(value => {
                                throw Error(`Element '${value}' has no class attribute. Is it select2 initialized?`);
                            });
                    });

                return result;

                function setErrorMessage(negated: boolean = false) {
                    getElementIdentifier()
                        .then(value => result.message = util.buildFailureMessage("toBeSelect2Disabled", negated, value));
                }

                // get element identifier from id or use the full HTML element if id is abcent
                function getElementIdentifier(): webdriver.promise.Promise<string> {
                    return actual.getAttribute("id")
                        .then(id => {
                            if (!id) throw Error();
                            return `#${id.toString()}`;
                        })
                        .thenCatch(() => {
                            return actual.getOuterHtml()
                                .then(html => {
                                    return html.toString();
                                });
                        });
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
         * returns true if the select2 select box is disabled
         */
        toBeSelect2Disabled(): boolean;
    }
}
