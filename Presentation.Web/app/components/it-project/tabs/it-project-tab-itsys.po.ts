import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import Select2Wrapper = require("../../../../Tests/object-wrappers/Select2Wrapper");
import RepeaterWrapper = require("../../../../Tests/object-wrappers/RepeaterWrapper");

class ItProjectEditTabItsys implements IPageObject {
    controllerVm = "";

    getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1/itsys");
    }

    // usage repeater
    usageRepeater = new RepeaterWrapper(`usage in ${this.controllerVm}systemUsages`);
    deleteLocator = by.css(".delete-usage");

    // usage selector
    usageSelect = new Select2Wrapper("#s2id_select-usage");
}

export = ItProjectEditTabItsys;
