// handle repeater directives
class RepeaterWrapper {
    locator: string;
    repeater: protractor.ElementArrayFinder;

    /**
     * instantiate repeater directive wrapper.
     *
     * @param {string} [locator] repeater locator like "post in posts"
     */
    constructor(locator: string) {
        this.locator = locator;

        this.repeater = element.all(by.repeater(this.locator));
    }

    /**
     * select elements in first repeated item.
     *
     * @param {webdriver.Locator} [locator] locator for elements to select in first repeated item
     */
    selectFirst(locator: webdriver.Locator): protractor.ElementArrayFinder {
         return this.repeater.first().all(locator);
    }

    /**
     * select elements in repeated item.
     *
     * @param {number} [index] index of repeated item to select
     * @param {webdriver.Locator} [locator] locator for elements to select in first repeated item
     */
    select(index: number, locator: webdriver.Locator): protractor.ElementArrayFinder {
        return this.repeater.get(index).all(locator);
    }
}

export = RepeaterWrapper;
