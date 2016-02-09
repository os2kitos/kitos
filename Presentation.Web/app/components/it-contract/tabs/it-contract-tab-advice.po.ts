import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import RepeaterWrapper = require("../../../../Tests/object-wrappers/RepeaterWrapper");
import Select2Wrapper = require("../../../../Tests/object-wrappers/Select2Wrapper");

class ItContractEditTabAdvicePo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/contract/edit/1/advice");
    }

    // advice repeater
    public adviceRepeater = new RepeaterWrapper("advice in advices");

    // active locator
    public activeLocator = by.css(".advice-active");

    // name locator
    public nameLocator = by.css(".advice-name");

    // date locator
    public dateLocator = by.css("input.advice-date");

    // receiver locator
    public receiverLocator = new Select2Wrapper(".advice-receiver.select2-container");

    // role locator
    public roleLocator = new Select2Wrapper(".advice-role.select2-container");

    // subject locator
    public subjectLocator = by.css(".advice-subject");

    // delete button locator
    public deleteButtonLocator = by.css(".delete-advice");

    // addButton
    public addButton = element(by.css("#add-advice"));
}

export = ItContractEditTabAdvicePo;
