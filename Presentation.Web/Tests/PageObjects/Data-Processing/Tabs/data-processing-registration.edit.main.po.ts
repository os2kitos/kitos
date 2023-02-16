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

    getSaveSubDataProcessorButton() {
        console.log("Getting the save SubDataProcessor button");
        return element(by.id("save-sub-data-processor-btn"));
    }


    getAddSubDataProcessorButton() {
        return element(by.id("create-sub-data-processor-btn"));
    }

    getRemoveSubDataProcessorButton(dpName: string) {
        return element(by.xpath(`${this.getSubDpRowExpression(dpName)}//*/button[2]`));
    }

    getEditSubDataProcessorButton(dpName: string) {
        return element(by.xpath(`${this.getSubDpRowExpression(dpName)}//*/button[1]`));
    }

    private getThirdCountryRowExpression(dpName: string) {
        return `//*/table[@id="insecureThirdCountriesTable"]//*/td[text()="${dpName}"]/..`;
    }

    getThirdCountryProcessorRow(dpName: string) {
        return element(by.xpath(this.getThirdCountryRowExpression(dpName)));
    }

    getRemoveThirdCountryButton(dpName: string) {
        return element(by.xpath(`${this.getThirdCountryRowExpression(dpName)}//button`));
    }

    getIsAgreementConcludedField() {
        return element(by.id("agreementConcluded"));
    }

    getAgreementConcludedAtDateField() {
        return element(by.id("agreementConcludedAt")).element(by.tagName("input"));
    }

    getDataResponsibleRemark() {
        return element(by.id("dataResponsibleRemark_remark"));
    }

    getAgreementConcludedRemark() {
        return element(by.id("agreementConcludedRemark_remark"));
    }

}
export = DataProcessingRegistrationEditMainPageObject;
