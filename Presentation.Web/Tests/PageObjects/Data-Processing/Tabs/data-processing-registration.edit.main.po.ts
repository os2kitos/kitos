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

    private getDpRowExpression(dpName: string) {
        return `//*/table[@id="dpTable"]//*/td[text()="${dpName}"]/..`;
    }

    getDataProcessorRow(dpName: string) {
        return element(by.xpath(this.getDpRowExpression(dpName)));
    }

    getRemoveDataProcessorButton(dpName: string) {
        return element(by.xpath(`${this.getDpRowExpression(dpName)}//button`));
    }

    private getSubDpRowExpression(dpName: string) {
        return `//*/table[@id="subDpTable"]//*/td[text()="${dpName}"]/..`;
    }

    getSubDataProcessorRow(dpName: string) {
        return element(by.xpath(this.getSubDpRowExpression(dpName)));
    }

    getRemoveSubDataProcessorButton(dpName: string) {
        return element(by.xpath(`${this.getSubDpRowExpression(dpName)}//button`));
    }

    getIsAgreementConcludedField() {
        return element(by.id("agreementConcluded"));
    }

    getAgreementConcludedAtDateField() {
        return element(by.id("agreementConcludedAt"));
    }

}
export = DataProcessingRegistrationEditMainPageObject;
