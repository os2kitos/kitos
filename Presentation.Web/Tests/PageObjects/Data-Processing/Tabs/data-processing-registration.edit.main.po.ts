import PageObject = require("../../IPageObject.po");
import NavigationHelper = require("../../../Utility/NavigationHelper");
import CssLocatorHelper = require("../../../Object-wrappers/CSSLocatorHelper");
import DataProcessingRegistrationNavigation =
require("../../../Helpers/SideNavigation/DataProcessingRegistrationNavigation");

class DataProcessingRegistrationEditMainPageObject {
    private navigationHelper = new NavigationHelper();
    private cssHelper = new CssLocatorHelper();

    refreshPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.refreshPage();
    }

    getMainPage() {
        DataProcessingRegistrationNavigation.mainPage();
    }

    getReferencePage() {
        DataProcessingRegistrationNavigation.referencePage();
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

    getDataProcessorRow(dpName: string) {
        return element(by.xpath(`//*/td[text()="${dpName}"]/..`));
    }

    getRemoveDataProcessorButton(dpName: string) {
        return element(by.xpath(`//*/td[text()="${dpName}"]/..//button`));
    }

    getIsAgreementConcludedField() {
        return element(by.id("agreementConcluded"));
    }

    getAgreementConcludedAtDateField() {
        return element(by.id("agreementConcludedAt"));
    }

}
export = DataProcessingRegistrationEditMainPageObject;
