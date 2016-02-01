import IPageObject = require("../../../../../Tests/object-wrappers/IPageObject.po");
import RepeaterWrapper = require("../../../../../Tests/object-wrappers/RepeaterWrapper");
import Select2Wrapper = require("../../../../../Tests/object-wrappers/Select2Wrapper");

class ItSystemUsageTabProjPo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/system/usage/1/proj");
    }

    // project selector
    public projectSelector = new Select2Wrapper("#s2id_project-selector");

    // project repeater
    public projectRepeater = new RepeaterWrapper("project in itProjects");
    public deleteLocator = by.css(".delete-project");
}

export = ItSystemUsageTabProjPo;
