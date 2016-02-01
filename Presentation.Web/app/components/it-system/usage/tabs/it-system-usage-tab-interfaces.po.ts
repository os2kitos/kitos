import IPageObject = require("../../../../../Tests/object-wrappers/IPageObject.po");
import RepeaterWrapper = require("../../../../../Tests/object-wrappers/RepeaterWrapper");

class ItSystemUsageTabInterfacesPo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/system/usage/1/interfaces");
    }

    // usage repeater
    public usageRepeater = new RepeaterWrapper("usage in interfaceUsages");

    // wishFor locator
    public wishForLocator = by.css("[data-field=\"isWishedFor\"]");
}

export = ItSystemUsageTabInterfacesPo;
