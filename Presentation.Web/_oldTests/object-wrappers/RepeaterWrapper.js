"use strict";
// handle repeater directives
var RepeaterWrapper = /** @class */ (function () {
    /**
     * instantiate repeater directive wrapper.
     *
     * @param {string} [locator] repeater locator like "post in posts"
     */
    function RepeaterWrapper(locator) {
        this.locator = locator;
        this.repeater = element.all(by.repeater(this.locator));
    }
    /**
     * select elements in first repeated item.
     *
     * @param {webdriver.Locator} [locator] locator for elements to select in first repeated item
     */
    RepeaterWrapper.prototype.selectFirst = function (locator) {
        return this.repeater.first().all(locator);
    };
    /**
     * select elements in repeated item.
     *
     * @param {number} [index] index of repeated item to select
     * @param {webdriver.Locator} [locator] locator for elements to select in first repeated item
     */
    RepeaterWrapper.prototype.select = function (index, locator) {
        return this.repeater.get(index).all(locator);
    };
    /**
     * get count of repetitions in repeater
     *
     * @returns Promise that resolves to the number of repetitions in repeater
     */
    RepeaterWrapper.prototype.count = function () {
        return this.repeater.count();
    };
    /**
     * calls the input function on each repeated item in repeater.
     *
     * @param {function(ElementFinder)} [fn] Input function
     */
    RepeaterWrapper.prototype.each = function (fn) {
        return this.repeater.each(fn);
    };
    return RepeaterWrapper;
}());
module.exports = RepeaterWrapper;
