import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import Select2Wrapper = require("../../../../Tests/object-wrappers/Select2Wrapper");
import RepeaterWrapper = require("../../../../Tests/object-wrappers/RepeaterWrapper");

class ItProjectEditTabKle implements IPageObject {
    controllerVm: string = "";

    getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/project/edit/1/kle");
    }

    // main group
    mainGroupSelect = new Select2Wrapper("#s2id_maingroup");

    // group
    groupSelect = new Select2Wrapper("#s2id_group");

    // change task view
    changeTaskViewElement = element(by.css("#change-task-view"));

    // task repeater
    taskRepeater = new RepeaterWrapper("task in " + this.controllerVm + "tasklist");
    checkboxLocator = by.css("input[type=checkbox]");

    // select buttons
    selectAllPagesElement = element(by.css("#select-all-pages"));
    selectAllElement = element(by.css("#select-all"));
    deselectAllElement = element(by.css("#deselect-all"));
    deselectAllPages = element(by.css("#deselect-all-pages"));
}

export = ItProjectEditTabKle;
