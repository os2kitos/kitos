import IPageObject = require("../IPageObject.po");
import KendoToolbarHelper = require("../../Helpers/KendoToolbarHelper");
import KendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper")
import CSSLocatorHelper = require("../../object-wrappers/CSSLocatorHelper")
var byDataElementType = new CSSLocatorHelper().byDataElementType;

class UsersPage implements IPageObject {

    public getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl + "/#/organization/user");
    }

    public kendoToolbarHelper = new KendoToolbarHelper();
    public kendoToolbarWrapper = new KendoToolbarWrapper();
    public createUserButton = element(byDataElementType('subnav-create-user-button'));
    public hasAPiCheckBox = element(by.model("ctrl.vm.hasApi"));
    public mainGridAllTableRows = element.all(by.id("mainGrid")).all(by.tagName("tr"));

}

export = UsersPage;