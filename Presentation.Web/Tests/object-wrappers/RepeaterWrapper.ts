// handle repeater directives
class RepeaterWrapper {
    locator: string;
    repeater: protractor.ElementArrayFinder;

    /**
     * instantiate repeater directive wrapper.
     *
     * @param locator repeater locator like "post in posts"
     */
    constructor(locator: string) {
        this.locator = locator;

        this.repeater = element.all(by.repeater(this.locator));
    }

    /**
     * select elements in first repeated item.
     */
    selectFirst(cssLocator: string): protractor.ElementArrayFinder {
        return this.repeater.first().all(by.css(cssLocator));
    }
}

export = RepeaterWrapper;
