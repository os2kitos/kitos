import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import Select2Wrapper = require("../../../../Tests/object-wrappers/Select2Wrapper");
import RepeaterWrapper = require("../../../../Tests/object-wrappers/RepeaterWrapper");

class ItContractEditTabSystemsPo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/contract/edit/1/systems");
    }

    // systemUsage selector
    public systemUsageSelector = new Select2Wrapper("#s2id_new-system-usage");

    // systemUsage repeater
    public systemUsageRepeater = new RepeaterWrapper("associatedSystemUsage in associatedSystemUsages");

    // deleteUsage locator
    public deleteUsageLocator = by.css(".delete-system-usage");

    // newInterfaceSystemUsage selector
    public newInterfaceSystemUsageSelector = new Select2Wrapper("#s2id_new-interface-system-usage");

    // newInterfaceUsageType selector
    public newInterfaceUsageTypeSelector = new Select2Wrapper("#s2id_new-interface-usage-type");

    // newInterfaceInterfaceUsage selector
    public newInterfaceInterfaceUsageSelector = new Select2Wrapper("#s2id_new-interface-interface-usage");

    // interfaceExhibit repeater
    public interfaceExhibitRepeater = new RepeaterWrapper("exhibit in exhibitedInterfaces");

    // deleteInterfaceExhibit locator
    public deleteInterfaceExhibitLocator = by.css(".delete-interface-exhibit");

    // interface repeater
    public interfaceRepeater = new RepeaterWrapper("used in usedInterfaces");

    // deleteInterfaceUsage locator
    public deleteInterfaceUsageLocator = by.css(".delete-interface-usage");
}

export = ItContractEditTabSystemsPo;
