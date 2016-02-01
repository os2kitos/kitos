import IPageObject = require("../../../../../Tests/object-wrappers/IPageObject.po");
import RepeaterWrapper = require("../../../../../Tests/object-wrappers/RepeaterWrapper");
import Select2Wrapper = require("../../../../../Tests/object-wrappers/Select2Wrapper");

class ItSystemUsageTabOrgPo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/system/usage/1/org");
    }

    // responsible organization
    public responsibleOrgSelector = new Select2Wrapper("#s2id_responsible-org");

    // phase name repeater
    public orgUnitsTreeRepeater = new RepeaterWrapper("orgUnit in orgUnitsTree");
    public orgUnitLocator = by.model("orgUnit.selected");
}

export = ItSystemUsageTabOrgPo;
