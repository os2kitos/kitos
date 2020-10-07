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

    getLatestOversightCompletedDate() {
        return element(by.id("latestOversightCompletedDate")).element(by.tagName("input"));
    }

}
export = DataProcessingRegistrationEditOversightPageObject;
