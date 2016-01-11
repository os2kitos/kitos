// handle select2 wrapping for protractor
// http://stackoverflow.com/questions/26504623/protractor-how-to-test-select2?answertab=votes#tab-top
class Select2Wrapper {
    cssSelector: string;
    elementSelector: string;
    element: protractor.ElementFinder;
    options: protractor.ElementArrayFinder;
    closeSelector: string;

    constructor(cssLocator: string) {
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
    selectFirst(query?: string): webdriver.promise.Promise<void> {
        this.isPresent();

        if (query) {
            this.writeQuery(query);
        } else {
            this.click();
        }

        this.waitForOptions();

        return this.options.first().click();
    }

    /**
     * write query to search input
     *
     * @param {string} [query] Search query to input
     *
     * @throws error if no options are found
     *
     * @return promise that resolves when options are found
     */
    writeQuery(query: string): webdriver.promise.Promise<void> {
        return this.click()
            .then(() => browser.driver
                .switchTo()
                .activeElement()
                .sendKeys(query)
                .then(() =>
                    browser.driver.wait(() => browser.driver.executeScript<boolean>("return $.active === 0;"), 2000)))
            .then(() => this.waitForOptions());
    }

    /**
     * wait for dropdown options
     *
     * @throws error if no options are found before timeout
     *
     * @return promise that resolves when options are found
     */
    waitForOptions(): webdriver.promise.Promise<void> {
        return browser.driver.wait(() => this.options.count().then(count => count > 0), 2000)
            .then(() => {
                return;
            }, err => {
                throw new Error(`No options found for select2 selector '${this.cssSelector}'`);
            });
    }

    /**
     * deselect dropdown
     *
     * @throws error if nothing is selected
     *
     * @return promise that resolves when close link is clicked
     */
    deselect(): webdriver.promise.Promise<void> {
        return $(this.closeSelector)
            .click()
            .then(() => { return; }, err => {
                throw new Error(`Can't deselect. Nothing is selected for select2 selector '${this.cssSelector}'`);
            });
    }

    /**
     * detect if dropdown is disabled
     *
     * @return promise that resolves to a boolean indicating whether the dropdown is disabled
     */
    isDisabled(): webdriver.promise.Promise<boolean> {
        return $(this.cssSelector + ".select2-container-disabled").isPresent();
    }

    /**
     * detect if selector is present in the DOM
     *
     * @throws error if not present
     */
    isPresent(): webdriver.promise.Promise<boolean> {
        return this.element.isPresent()
            .then(present => {
                if (!present) {
                    throw Error(`select2 element not found using selector '${this.cssSelector}'`);
                }
            })
            .then(() => $(this.elementSelector).isPresent())
            .then(present => {
                if (!present) {
                    throw Error(`select2 element not found using selector '${this.cssSelector}'. Is the element select2 initialized?`);
                }
            })
            .then(() => true);
    }

    /**
     * click dropdown
     *
     * @return promise that resolves when element is clicked
     */
    click(): webdriver.promise.Promise<void> {
        return browser.driver.executeScript<void>("$(arguments[\"0\"]).mousedown();", (this.elementSelector));
    }
}

export = Select2Wrapper;
