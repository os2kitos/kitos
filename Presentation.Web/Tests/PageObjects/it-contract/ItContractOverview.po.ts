import kendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper");
import navigationBarWrapper = require("../../object-wrappers/NavigationBarWrapper");
import subNavigationBarWrapper = require("../../object-wrappers/SubNavigationBarWrapper");
import PageObject = require("../IPageObject.po");

class ItSystemEditPo implements PageObject {

    getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl + "/#/contract/overview");
    }

    kendoToolbarWrapper = new kendoToolbarWrapper();
    navigationBarWrapper = new navigationBarWrapper();
    subNavigationBarWrapper = new subNavigationBarWrapper();

    createItContract = element(by.buttonText("Opret IT Kontrakt"));

}
export = ItSystemEditPo;