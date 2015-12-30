import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import Select2Wrapper = require("../../../../Tests/object-wrappers/Select2Wrapper");

class ItProjectEditTabStrategy implements IPageObject {
    controllerVm: string = "projectStatusVm";

    getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1/strategy");
    }

    // joint municipal project
    jointMunicipalSelector = new Select2Wrapper("#s2id_jointDrop");

    // common public project
    commonPublicSelector = new Select2Wrapper("#s2id_commonDrop");
}

export = ItProjectEditTabStrategy;
