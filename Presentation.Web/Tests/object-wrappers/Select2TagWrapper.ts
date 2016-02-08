// handle select2 wrapping for protractor
// http://stackoverflow.com/questions/26504623/protractor-how-to-test-select2?answertab=votes#tab-top
class Select2TagWrapper {
    public cssSelector: string;
    public element: protractor.ElementFinder;
    public options: protractor.ElementArrayFinder;
    public selectedOptionsSelector: string;

    constructor(cssLocator: string) {
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
    public selectFirst(query?: string): webdriver.promise.Promise<void> {
        this.isPresent();

        this.click();

        if (query) {
            browser.driver.switchTo().activeElement().sendKeys(query);
            browser.driver.wait(() => browser.driver.executeScript("return $.active === 0;"), 2000);
        }

        browser.driver.wait(() => this.options.count().then(count => count > 0), 2000)
            .thenCatch(err => {
                throw new Error(`No options found for select2 selector '${this.cssSelector}'`);
            });

        return this.options.first().click();
    }

    /**
     * Detect if select2 dropdown is disabled
     *
     * @return Promise that resolves to a boolean indicating if the dropdown is disabled or not.
     */
    public isDisabled(): webdriver.promise.Promise<boolean> {
        return $(this.cssSelector + ".select2-container-disabled").isPresent();
    }

    /**
     * detect if element is present in the DOM
     *
     * @throws error is not present.
     */
    public isPresent(): webdriver.promise.Promise<boolean> {
        return this.element.isPresent()
            .then(present => {
                if (!present) {
                    throw Error(`select2 element not found using selector '${this.cssSelector}'`);
                }
            })
            .then(() => true);
    }

    /**
     * click select2 dropdown
     *
     * @return Promise that resolves when element is clicked.
     */
    public click(): webdriver.promise.Promise<void> {
        return this.element.click();
    }

    /**
     * get selected options
     *
     * @return ElementArrayFinder with selected options
     */
    public selectedOptions(): protractor.ElementArrayFinder {
        return element.all(by.css(this.selectedOptionsSelector));
    }
}

export = Select2TagWrapper;
