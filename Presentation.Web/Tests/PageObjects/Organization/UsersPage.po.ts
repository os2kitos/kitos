import IPageObject = require("../IPageObject.po");
import KendoToolbarHelper = require("../../Helpers/KendoToolbarHelper");
import KendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper")

class UsersPage implements IPageObject {

    public getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl + "/#/organization/user");
    }

    public kendoToolbarHelper = new KendoToolbarHelper();
    public kendoToolbarWrapper = new KendoToolbarWrapper();
    public createUserButton = element(by.linkText("Opret Bruger"));
    public hasAPiCheckBox = element(by.model("ctrl.vm.hasApi"));

    public mainGridAllTableRows = element.all(by.id("mainGrid")).all(by.tagName("tr"));

}

export = UsersPage;