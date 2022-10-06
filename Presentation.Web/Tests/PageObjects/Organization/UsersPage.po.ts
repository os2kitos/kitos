import IPageObject = require("../IPageObject.po");
import KendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper")
import NavigationHelper = require("../../Utility/NavigationHelper");
import CssLocatorHelper = require("../../Object-wrappers/CSSLocatorHelper");

class UsersPage implements IPageObject {

    private navigationHelper = new NavigationHelper();
    private cssLocator = new CssLocatorHelper();

    getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/organization/user");
    }

    kendoToolbarWrapper = new KendoToolbarWrapper();
    createUserButton = element(by.linkText("Opret Bruger"));
    hasAPiCheckBox = element(by.model("ctrl.vm.hasApi"));
    hasRightsHolderAccessCheckBox = element(by.model("ctrl.vm.isRightsHolder"));
    hasStakeHolderAccessCheckBox = element(by.model("ctrl.vm.hasStakeHolderAccess"));
    mainGridAllTableRows = element.all(by.id("mainGrid")).all(by.tagName("tr"));
    getCreateUserButton() {
        return element(this.cssLocator.byDataElementType("createUserButton"));
    }

    getPrimaryStartUnitElementIds() {
        return [
            "index",
            "it-system.overview",
            "it-system.catalog",
            "it-contract.overview",
            "data-processing.overview",
            "organization.overview"
        ].map(x => x.replace(".", "_"));
    }
}

export = UsersPage;