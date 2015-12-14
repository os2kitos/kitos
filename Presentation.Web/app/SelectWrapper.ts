class SelectWrapper {
    element: protractor.ElementFinder;

    constructor(selector: webdriver.Locator) {
        this.element = element(selector);
    }

    getOptions() {
        return this.element.all(by.tagName("option"));
    }

    getSelectedOptions() {
        return this.element.all(by.css("option[selected=\"selected\"]"));
    }

    selectByValue(value: string) {
        return this.element.all(by.css("option[value=\"" + value + "\"")).click();
    }

    selectByPartialText(text: string) {
        return this.element.all(by.cssContainingText("option", text)).click();
    }
    
    selectByText(text: string) {
        return this.element.all(by.xpath("option[.=\"" + text + "\"")).click();
    }
}

export = SelectWrapper;
