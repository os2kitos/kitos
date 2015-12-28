import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import RepeaterWrapper = require("../../../../Tests/object-wrappers/RepeaterWrapper");
import Select2Wrapper = require("../../../../Tests/object-wrappers/Select2Wrapper");

class PageObject implements IPageObject {
    controllerVm: string = "projectRolesVm";

    getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1/roles");
    }

    // phase name repeater
    rightsRepeater = new RepeaterWrapper("right in " + this.controllerVm + ".right");
    rightRowLocator = by.css("tr");
    rightEditButtonLocator = by.css("tr a.edit-right");
    rightEditDeleteLocator = by.css("tr a.delete-right");
    rightEditRoleInputLocator = by.css("tr select");
    rightEditSaveButtonLocator = by.css("tr input[type=submit]");

    // add right role selector
    addRightRoleSelector = new Select2Wrapper("#s2id_add-right-role");

    // add right user selector
    addRightUserSelector = new Select2Wrapper("#add-right-user .select2-container");

}

export = PageObject;
