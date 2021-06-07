import PageObject = require("../../IPageObject.po");
import NavigationHelper = require("../../../Utility/NavigationHelper");
import CssLocatorHelper = require("../../../Object-wrappers/CSSLocatorHelper");
import DataProcessingRegistrationNavigation =
require("../../../Helpers/SideNavigation/DataProcessingRegistrationNavigation");

class DataProcessingRegistrationEditOversightPageObject {

    private navigationHelper = new NavigationHelper();
    private cssHelper = new CssLocatorHelper();

    refreshPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.refreshPage();
    }

    getOversightPage() {
        DataProcessingRegistrationNavigation.oversightPage();
    }
    
    getOversightIntervalOption() {
        return element(by.id("oversightInterval"));
    }

    getOversightIntervalRemark() {
        return element(by.id("oversightIntervalRemark_remark"));
    }

    getOversightOptionRemark() {
        return element(by.id("oversightOptionRemark_remark"));
    }

    getOversightOptionRow(oversightOptionName: string) {
        return element(by.xpath(this.getOversightOptionRowExpression(oversightOptionName)));
    }

    getRemoveOversightOptionButton(oversightOptionName: string) {
        return element(by.xpath(`${this.getOversightOptionRowExpression(oversightOptionName)}//button`));
    }

    private getOversightOptionRowExpression(oversightOptionName: string) {
        return `//*/table[@id="oversightTable"]//*/td[text()="${oversightOptionName}"]/..`;
    }

    getOversightCompleted() {
        return element(by.id("oversightCompleted"));
    }

    getOversightCompletedRemark() {
        return element(by.id("oversightCompletedRemark_remark"));
    }

    getAssignOversightDateButton() {
        return element(by.id("create-oversight-date"));
    }

    getRemoveOversightDateButton() {
        return element(by.xpath(`${this.getOversightDateRowExpression()}//button[@id='delete-oversight-date']`));
    }

    getOversightDateRow() {
        return element(by.xpath(this.getOversightDateRowExpression()));
    }

    getOversightDateRowDate() {
        return element(by.xpath(this.getOversightDateRowExpression())).element(this.cssHelper.byDataElementType("oversight-date"));
    }

    getOversightDateRowRemark() {
        return element(by.xpath(this.getOversightDateRowExpression())).element(this.cssHelper.byDataElementType("oversight-remark"));
    }

    getOversightDateModalDateField() {
        return element(by.id("date"));
    }

    getOversightDateModalRemarkField() {
        return element(by.id("remark"));
    }

    getOversightDateModalSaveButton() {
        return element(by.id("save"));
    }

    private getOversightDateRowExpression() {
        return `//*/table[@id="oversightDatesTable"]//*/input[@data-element-type="oversight-date"]/../..`;
    }

    getDpaMainNameHeader() {
        return element(this.cssHelper.byDataElementType("dpaMainDetailHeader"));
    }
}
export = DataProcessingRegistrationEditOversightPageObject;
