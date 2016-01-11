import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import Select2Wrapper = require("../../../../Tests/object-wrappers/Select2Wrapper");
import RepeaterWrapper = require("../../../../Tests/object-wrappers/RepeaterWrapper");

class ItProjectEditTabOrg implements IPageObject {
    controllerVm = "";

    getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1/org");
    }

    // responsible organization
    responsibleOrgSelector = new Select2Wrapper("#s2id_responsibleOrg");

    // phase name repeater
    orgUnitsTreeRepeater = new RepeaterWrapper(`orgUnit in ${this.controllerVm}orgUnitsTree`);
    orgUnitLocator = by.model("orgUnit.selected");

}

export = ItProjectEditTabOrg;
