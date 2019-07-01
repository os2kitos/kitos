beforeEach(function () {
    jasmine.addMatchers({
        "toHaveClass": function (util) {
            var compare = function (actual, expected) {
                var result = {
                    pass: null,
                    message: null
                };
                if (!actual.getAttribute) {
                    throw Error("Can't determine if element has class. Method getAttribute() is not defined. Are you expecting on a 'protractor.ElementArrayFinder'?");
                }
                result.pass = browser.wait(actual.getAttribute("class"), 2000)
                    .then(function (v) {
                    // find exact case-insensitive class
                    var re = new RegExp("(\w+)?(" + expected + ")(?!-)(\w+)?", "i");
                    var result = v.search(re) !== -1;
                    setErrorMessage(result, v); // negated on pass in case .not is used on matcher
                    return result;
                }, function () {
                    getElementIdentifier().then(function (v) {
                        throw Error("Can't determine if '" + v + "' has class. Method getAttribute() timed out.");
                    });
                });
                return result;
                // create error message from id, name, data-ng-model or use the full HTML element if others are abcent
                function getElementIdentifier() {
                    if (!actual.getAttribute) {
                        throw Error("Can't determine identifier for element. Method getAttribute() is undefined.");
                    }
                    return actual.getAttribute("id")
                        .then(function (id) {
                        if (!id)
                            throw Error();
                        return "#" + id.toString();
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
                        return actual.getAttribute("name")
                            .then(function (name) {
                            if (!name)
                                throw Error();
                            return name.toString();
                        });
                    })
                        .thenCatch(function () {
                        return actual.getOuterHtml()
                            .then(function (html) {
                            return html.toString();
                        });
                    });
                }
                function setErrorMessage(negated, message) {
                    if (negated === void 0) { negated = false; }
                    if (message === void 0) { message = null; }
                    getElementIdentifier()
                        .then(function (value) { return result.message = util.buildFailureMessage("toHaveClass", negated, value + (message ? ": \"" + message + "\"" : ""), expected); });
                }
            };
            return {
                compare: compare
            };
        }
    });
});
