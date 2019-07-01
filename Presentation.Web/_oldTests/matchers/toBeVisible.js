beforeEach(function () {
    jasmine.addMatchers({
        "toBeVisible": function (util) {
            var compare = function (actual) {
                var result = {
                    pass: null,
                    message: null
                };
                if (!actual.isDisplayed) {
                    getElementIdentifier().then(function (v) {
                        throw Error("Can't determine visibility of '" + v + "'. Method isDisplayed() is undefined.");
                    });
                }
                result.pass = browser.wait(actual.isDisplayed(), 2000)
                    .then(function (v) {
                    setErrorMessage(v); // negated on pass in case .not is used on matcher
                    return v;
                }, function () {
                    getElementIdentifier().then(function (v) {
                        throw Error("Can't determine visibility of '" + v + "'. Method isDisplayed() timed out.");
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
                        .then(function (value) { return result.message = util.buildFailureMessage("toBeVisible", negated, value); });
                }
            };
            return {
                compare: compare
            };
        }
    });
});
