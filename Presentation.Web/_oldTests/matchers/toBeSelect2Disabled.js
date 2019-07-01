beforeEach(function () {
    jasmine.addMatchers({
        "toBeSelect2Disabled": function (util) {
            var compare = function (actual) {
                var result = {
                    pass: null,
                    message: null
                };
                if (!actual.getAttribute) {
                    throw Error("Can't determine if element is disabled. Method getAttribute() is not defined. Are you expecting on a 'protractor.ElementArrayFinder'?");
                }
                result.pass = browser.wait(function () { return actual.getAttribute("class"); }, 2000)
                    .then(function (value) {
                    var pass = value.search(".select2-container-disabled") !== -1;
                    setErrorMessage(pass); // negated on pass if .not is used on matcher
                    return pass;
                }, function () {
                    getElementIdentifier()
                        .then(function (value) {
                        throw Error("Element '" + value + "' has no class attribute. Is it select2 initialized?");
                    });
                });
                return result;
                function setErrorMessage(negated) {
                    if (negated === void 0) { negated = false; }
                    getElementIdentifier()
                        .then(function (value) { return result.message = util.buildFailureMessage("toBeSelect2Disabled", negated, value); });
                }
                // get element identifier from id or use the full HTML element if id is abcent
                function getElementIdentifier() {
                    return actual.getAttribute("id")
                        .then(function (id) {
                        if (!id)
                            throw Error();
                        return "#" + id.toString();
                    })
                        .thenCatch(function () {
                        return actual.getOuterHtml()
                            .then(function (html) {
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
