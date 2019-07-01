"use strict";
// handle select2 wrapping for protractor
// http://stackoverflow.com/questions/26504623/protractor-how-to-test-select2?answertab=votes#tab-top
var Select2TagWrapper = /** @class */ (function () {
    function Select2TagWrapper(cssLocator) {
        this.cssSelector = cssLocator;
        this.element = $(this.cssSelector);
        this.options = element.all(by.css(".select2-results-dept-0"));
        this.selectedOptionsSelector = this.cssSelector + " .select2-search-choice";
    }
    /**
     * Select first element in select2 dropdown.
     *
     * @param query An optional search query for the dropdown.
     */
    Select2TagWrapper.prototype.selectFirst = function (query) {
        var _this = this;
        this.isPresent();
        this.click();
        if (query) {
            browser.driver.switchTo().activeElement().sendKeys(query);
            browser.driver.wait(function () { return browser.driver.executeScript("return $.active === 0;"); }, 2000);
        }
        browser.driver.wait(function () { return _this.options.count().then(function (count) { return count > 0; }); }, 2000)
            .thenCatch(function (err) {
            throw new Error("No options found for select2 selector '" + _this.cssSelector + "'");
        });
        return this.options.first().click();
    };
    /**
     * Detect if select2 dropdown is disabled
     *
     * @return Promise that resolves to a boolean indicating if the dropdown is disabled or not.
     */
    Select2TagWrapper.prototype.isDisabled = function () {
        return $(this.cssSelector + ".select2-container-disabled").isPresent();
    };
    /**
     * detect if element is present in the DOM
     *
     * @throws error is not present.
     */
    Select2TagWrapper.prototype.isPresent = function () {
        var _this = this;
        return this.element.isPresent()
            .then(function (present) {
            if (!present) {
                throw Error("select2 element not found using selector '" + _this.cssSelector + "'");
            }
        })
            .then(function () { return true; });
    };
    /**
     * click select2 dropdown
     *
     * @return Promise that resolves when element is clicked.
     */
    Select2TagWrapper.prototype.click = function () {
        return this.element.click();
    };
    /**
     * get selected options
     *
     * @return ElementArrayFinder with selected options
     */
    Select2TagWrapper.prototype.selectedOptions = function () {
        return element.all(by.css(this.selectedOptionsSelector));
    };
    return Select2TagWrapper;
}());
module.exports = Select2TagWrapper;
