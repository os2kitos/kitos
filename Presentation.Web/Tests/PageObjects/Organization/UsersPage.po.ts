import IPageObject = require("../IPageObject.po");
import KendoToolbarHelper = require("../../Helpers/KendoToolbarHelper");
import KendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper")
import NavigationHelper = require("../../Utility/NavigationHelper");

class UsersPage implements IPageObject {

    private navigationHelper = new NavigationHelper();

    public getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/organization/user");
    }

    public kendoToolbarHelper = new KendoToolbarHelper();
    public kendoToolbarWrapper = new KendoToolbarWrapper();
    public createUserButton = element(by.linkText("Opret Bruger"));
    public hasAPiCheckBox = element(by.model("ctrl.vm.hasApi"));
    public mainGridAllTableRows = element.all(by.id("mainGrid")).all(by.tagName("tr"));

}

export = UsersPage;