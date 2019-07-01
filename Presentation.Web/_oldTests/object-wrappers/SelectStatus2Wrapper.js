"use strict";
// handle selectStatus2 directive dropdowns
var SelectStatus2Wrapper = /** @class */ (function () {
    /**
     * instantiate selectStatus2 directive wrapper.
     *
     * @param {string} [cssLocator] CSS locator for directive element.
     */
    function SelectStatus2Wrapper(cssLocator) {
        this.cssSelector = cssLocator;
        this.dropdownElement = $(this.cssSelector + " a.dropdown-toggle");
        this.options = element.all(by.css(cssLocator + " .traffic-light li a"));
    }
    /**
     * select first element.
     */
    SelectStatus2Wrapper.prototype.selectFirst = function () {
        var _this = this;
        this.dropdownElement.click().then(null, function () {
            throw Error("No active selectStatus2 directive found with selector '" + _this.cssSelector + "'");
        });
        browser.driver.wait(function () { return _this.options.count().then(function (count) { return count > 0; }); }, 2000)
            .then(null, function (err) { return _this.createError("No options found.\n  " + err); });
        return this.options.first().click();
    };
    /**
     * select element by index.
     *
     * @param {number} [index] Options index on dropdown starting on 0.
     */
    SelectStatus2Wrapper.prototype.select = function (index) {
        var _this = this;
        if (index < 0) {
            this.createError("Index must be positive: " + index);
        }
        this.dropdownElement.click().then(null, function () {
            throw Error("No active selectStatus2 directive found with selector '" + _this.cssSelector + "'");
        });
        browser.driver.wait(function () { return _this.options.count().then(function (count) { return count > 0; }); }, 2000)
            .then(function () {
            return _this.options.count();
        }, function (err) {
            return _this.createError("No options found.\n  " + err);
        })
            .then(function (count) {
            if (index >= count) {
                _this.createError("Index out of range: " + index + " Range: [0 - " + count + "]");
            }
        });
        return this.options.get(index).click();
    };
    /**
     * is directive disabled
     *
     * @return Promise that resolves to a boolean indicating if the element is disabled
     */
    SelectStatus2Wrapper.prototype.isDisabled = function () {
        return $(this.cssSelector + " div:not(.dropdown)").isDisplayed();
    };
    /**
     * throw error message
     */
    SelectStatus2Wrapper.prototype.createError = function (message) {
        throw Error("selectStatus2 selector: " + this.cssSelector + "\n  " + message + "\n");
    };
    return SelectStatus2Wrapper;
}());
module.exports = SelectStatus2Wrapper;
