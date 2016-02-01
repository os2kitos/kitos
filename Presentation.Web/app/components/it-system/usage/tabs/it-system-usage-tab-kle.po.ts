import IPageObject = require("../../../../../Tests/object-wrappers/IPageObject.po");
import Select2Wrapper = require("../../../../../Tests/object-wrappers/Select2Wrapper");
import RepeaterWrapper = require("../../../../../Tests/object-wrappers/RepeaterWrapper");

class ItSystemUsageTabKle implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/system/usage/1/kle");
    }

    // main group
    public mainGroupSelect = new Select2Wrapper("#s2id_maingroup");

    // group
    public groupSelect = new Select2Wrapper("#s2id_group");

    // change task view
    public changeTaskViewElement = element(by.css("#change-task-view"));

    // task repeater
    public taskRepeater = new RepeaterWrapper("task in tasklist");
    public checkboxLocator = by.css("input[type=checkbox]");

    // select buttons
    public selectAllPagesElement = element(by.css("#select-all-pages"));
    public selectAllElement = element(by.css("#select-all"));
    public deselectAllElement = element(by.css("#deselect-all"));
    public deselectAllPages = element(by.css("#deselect-all-pages"));
}

export = ItSystemUsageTabKle;
