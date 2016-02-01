import IPageObject = require("../../../../../Tests/object-wrappers/IPageObject.po");
import RepeaterWrapper = require("../../../../../Tests/object-wrappers/RepeaterWrapper");
import Select2Wrapper = require("../../../../../Tests/object-wrappers/Select2Wrapper");

class ItSystemUsageTabRolesPo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/system/usage/1/roles");
    }

    // rights repeater
    public rightsRepeater = new RepeaterWrapper("right in rights");
    public rightRowLocator = by.css("tr");
    public rightEditButtonLocator = by.css("tr a.edit-right");
    public rightEditDeleteLocator = by.css("tr a.delete-right");
    public rightEditRoleInputLocator = by.css("tr select");
    public rightEditSaveButtonLocator = by.css("tr input[type=submit]");

    // add right role selector
    public addRightRoleSelector = new Select2Wrapper("#s2id_add-right-role");

    // add right user selector
    public addRightUserSelector = new Select2Wrapper("#add-right-user .select2-container");
}

export = ItSystemUsageTabRolesPo;
