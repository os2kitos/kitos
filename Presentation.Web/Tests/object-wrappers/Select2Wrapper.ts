// handle select2 wrapping for protractor
// http://stackoverflow.com/questions/26504623/protractor-how-to-test-select2?answertab=votes#tab-top
class Select2Wrapper {
    selector: string;
    options: protractor.ElementArrayFinder;

    constructor(cssLocator: string) {
        this.selector = cssLocator + " a.select2-choice";
        this.options = element.all(by.css(".select2-results-dept-0"));
    }

    /**
     * Select first element in select2 dropdown.
     *
     * @param query An optional search query for the dropdown.
     */
    selectFirst(query?: string): webdriver.promise.Promise<void> {
        browser.driver.executeScript("$(arguments[\"0\"]).mousedown();", (this.selector));

        if (query) {
            browser.driver.switchTo().activeElement().sendKeys(query);
            browser.driver.wait(() => browser.driver.executeScript("return $.active === 0;"), 2000);
        }

        browser.driver.wait(() => this.options.count().then(count => count > 0), 2000);

        return this.options.first().click();
    }
}

export = Select2Wrapper;
