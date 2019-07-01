beforeEach(function () {
    jasmine.addMatchers({
        "toBeChecked": function (util) {
            var compare = function (actual) {
                var result = {
                    pass: null,
                    message: null
                };
                if (!actual.isSelected) {
                    throw Error("Can't determine if element is cheked. Method isSelected() is not defined. Are you expecting on a 'protractor.ElementArrayFinder'?");
                }
                result.pass = actual.isSelected().then(function (v) {
                    setErrorMessage(v);
                    return v;
                });
                return result;
                // get element identifier from id, name, data-ng-model or use the full HTML element if others are abcent
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
                        .then(function (value) { return result.message = util.buildFailureMessage("toBeChecked", negated, value); });
                }
                ;
            };
            return {
                compare: compare
            };
        }
    });
});
