import PageObject = require("../../IPageObject.po");
import NavigationHelper = require("../../../Utility/NavigationHelper");
import CssLocatorHelper = require("../../../Object-wrappers/CSSLocatorHelper");
import DataProcessingAgreementNavigation = require("../../../Helpers/SideNavigation/DataProcessingAgreementNavigation");

class DataProcessingAgreementEditMainPageObject {
    private navigationHelper = new NavigationHelper();
    private cssHelper = new CssLocatorHelper();

    refreshPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.refreshPage();
    }

    getMainPage() {
        DataProcessingAgreementNavigation.mainPage();
    }

    getReferencePage() {
        DataProcessingAgreementNavigation.referencePage();
    }

    getDpaMainNameInput() {
        return element(this.cssHelper.byDataElementType("dpaMainName"));
    }

    getDpaMainNameHeader() {
        return element(this.cssHelper.byDataElementType("dpaMainDetailHeader"));
    }

    getDpaDeleteButton() {
        return element(this.cssHelper.byDataElementType("removeDataProcessingRegistrationButton"));
    }

}
export = DataProcessingAgreementEditMainPageObject;