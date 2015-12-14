// handle select2 wrapping for protractor
// http://stackoverflow.com/questions/26504623/protractor-how-to-test-select2?answertab=votes#tab-top
class Select2Wrapper {
    selector: string;
    options: protractor.ElementArrayFinder;

    constructor(cssLocator: string) {
        this.selector = cssLocator + " a.select2-choice";
        this.options = element.all(by.css(".select2-results-dept-0"));
    }

    selectFirst(query?: string) {
        browser
            .driver
            .executeScript("$(arguments[\"0\"]).mousedown();", (this.selector));
            
        if (query) {
            browser.driver.switchTo().activeElement().sendKeys(query);

            browser
                .driver
                .wait(() => browser.driver.executeScript('return $.active === 0;'), 2000);
        }
        
        //this.options.first().click();
    }
}

export = Select2Wrapper;
