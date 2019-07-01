"use strict";
// handle select2 wrapping for protractor
// http://stackoverflow.com/questions/26504623/protractor-how-to-test-select2?answertab=votes#tab-top
var Select2Wrapper = /** @class */ (function () {
    function Select2Wrapper(cssLocator) {
        this.cssSelector = cssLocator;
        this.elementSelector = cssLocator + " a.select2-choice";
        this.closeSelector = cssLocator + " .select2-search-choice-close";
        this.element = $(this.cssSelector);
        this.options = element.all(by.css(".select2-results-dept-0"));
    }
    /**
     * select first element in dropdown
     *
     * @param {string} [query] An optional search query for the dropdown.
     *
     * @throws error if no options are found
     */
    Select2Wrapper.prototype.selectFirst = function (query) {
        this.isPresent();
        if (query) {
            this.writeQuery(query);
        }
        else {
            this.click();
        }
        this.waitForOptions();
        return this.options.first().click();
    };
    /**
     * write query to search input
     *
     * @param {string} [query] Search query to input
     *
     * @throws error if no options are found
     *
     * @return promise that resolves when options are found
     */
    Select2Wrapper.prototype.writeQuery = function (query) {
        var _this = this;
        return this.click()
            .then(function () { return browser.driver
            .switchTo()
            .activeElement()
            .sendKeys(query)
            .then(function () {
            return browser.driver.wait(function () { return browser.driver.executeScript("return $.active === 0;"); }, 2000);
        }); })
            .then(function () { return _this.waitForOptions(); });
    };
    /**
     * wait for dropdown options
     *
     * @throws error if no options are found before timeout
     *
     * @return promise that resolves when options are found
     */
    Select2Wrapper.prototype.waitForOptions = function () {
        var _this = this;
        return browser.driver.wait(function () { return _this.options.count().then(function (count) { return count > 0; }); }, 2000)
            .then(function () {
            return;
        }, function (err) {
            throw new Error("No options found for select2 selector '" + _this.cssSelector + "'");
        });
    };
    /**
     * deselect dropdown
     *
     * @throws error if nothing is selected
     *
     * @return promise that resolves when close link is clicked
     */
    Select2Wrapper.prototype.deselect = function () {
        var _this = this;
        return $(this.closeSelector)
            .click()
            .then(function () { return; }, function (err) {
            throw new Error("Can't deselect. Nothing is selected for select2 selector '" + _this.cssSelector + "'");
        });
    };
    /**
     * detect if dropdown is disabled
     *
     * @return promise that resolves to a boolean indicating whether the dropdown is disabled
     */
    Select2Wrapper.prototype.isDisabled = function () {
        return $(this.cssSelector + ".select2-container-disabled").isPresent();
    };
    /**
     * detect if selector is present in the DOM
     *
     * @throws error if not present
     */
    Select2Wrapper.prototype.isPresent = function () {
        var _this = this;
        return this.element.isPresent()
            .then(function (present) {
            if (!present) {
                throw Error("select2 element not found using selector '" + _this.cssSelector + "'");
            }
        })
            .then(function () { return $(_this.elementSelector).isPresent(); })
            .then(function (present) {
            if (!present) {
                throw Error("select2 element not found using selector '" + _this.cssSelector + "'. Is the element select2 initialized?");
            }
        })
            .then(function () { return true; });
    };
    /**
     * click dropdown
     *
     * @return promise that resolves when element is clicked
     */
    Select2Wrapper.prototype.click = function () {
        return browser.driver.executeScript("$(arguments[\"0\"]).mousedown();", (this.elementSelector));
    };
    return Select2Wrapper;
}());
module.exports = Select2Wrapper;
