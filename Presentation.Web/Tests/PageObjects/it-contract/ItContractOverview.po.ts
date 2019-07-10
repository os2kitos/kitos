import IPageObject = require("../../object-wrappers/IPageObject.po");
import kendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper");
import navigationBarWrapper = require("../../object-wrappers/NavigationBarWrapper");
import subNavigationBarWrapper = require("../../object-wrappers/SubNavigationBarWrapper");

class ItSystemEditPo implements IPageObject {

    getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl + "/#/contract/overview");
    }

    kendoToolbarWrapper = new kendoToolbarWrapper();
    navigationBarWrapper = new navigationBarWrapper();
    subNavigationBarWrapper = new subNavigationBarWrapper();

    createItContract = element(by.buttonText("Opret IT Kontrakt"));

}
export = ItSystemEditPo;