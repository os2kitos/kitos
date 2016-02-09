// handle repeater directives
class RepeaterWrapper {
    public repeater: protractor.ElementArrayFinder;

    /**
     * instantiate repeater directive wrapper.
     *
     * @param {string} [locator] repeater locator like "post in posts"
     */
    constructor(public locator: string) {

        this.repeater = element.all(by.repeater(this.locator));
    }

    /**
     * select elements in first repeated item.
     *
     * @param {webdriver.Locator} [locator] locator for elements to select in first repeated item
     */
    public selectFirst(locator: webdriver.Locator): protractor.ElementArrayFinder {
        return this.repeater.first().all(locator);
    }

    /**
     * select elements in repeated item.
     *
     * @param {number} [index] index of repeated item to select
     * @param {webdriver.Locator} [locator] locator for elements to select in first repeated item
     */
    public select(index: number, locator: webdriver.Locator): protractor.ElementArrayFinder {
        return this.repeater.get(index).all(locator);
    }

    /**
     * get count of repetitions in repeater
     *
     * @returns Promise that resolves to the number of repetitions in repeater
     */
    public count(): webdriver.promise.Promise<number> {
        return this.repeater.count();
    }

    /**
     * calls the input function on each repeated item in repeater.
     *
     * @param {function(ElementFinder)} [fn] Input function
     */
    public each(fn: (element: protractor.ElementFinder, index: number) => void): void {
        return this.repeater.each(fn);
    }
}

export = RepeaterWrapper;
