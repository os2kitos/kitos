beforeEach(function () {
    jasmine.addMatchers({
        "toBeDisabled": function (util) {
            var compare = function (actual) {
                var result = {
                    pass: null,
                    message: null
                };
                if (!actual.getAttribute) {
                    throw Error("Can't determine if element is disabled. Method getAttribute() is not defined. Are you expecting on a 'protractor.ElementArrayFinder'?");
                }
                // protractor always returns true if disabled attribute is pressent
                // otherwise it rejects the promise
                result.pass = browser.wait(function () { return actual.getAttribute("disabled"); }, 2000)
                    .then(function () {
                    setErrorMessage(true); // used when .not is used on matcher
                    return true;
                }, function () {
                    setErrorMessage();
                    return false;
                });
                return result;
                // get element identifier from id, name, data-ng-model or use the full HTML element if others are abcent
                function getElementIdentifier() {
                    return actual.getAttribute("id")
                        .then(function (id) {
                        if (!id)
                            throw Error();
                        return "#" + id.toString();
                    })
                        .thenCatch(function () {
                        return actual.getAttribute("name")
                            .then(function (name) {
                            if (!name)
                                throw Error();
                            return name.toString();
                        });
                    })
                        .thenCatch(function () {
                        return actual.getAttribute("data-ng-model")
                            .then(function (model) {
                            if (!model)
                                throw Error();
                            return model.toString();
                        });
                    })
                        .thenCatch(function () {
                        return actual.getOuterHtml()
                            .then(function (html) {
                            return html.toString();
                        });
                    });
                }
                function setErrorMessage(negated) {
                    if (negated === void 0) { negated = false; }
                    getElementIdentifier()
                        .then(function (value) { return result.message = util.buildFailureMessage("toBeDisabled", negated, value); });
                }
            };
            return {
                compare: compare
            };
        }
    });
});
