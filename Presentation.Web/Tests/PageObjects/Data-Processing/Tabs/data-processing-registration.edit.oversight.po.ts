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
    getOversightIntervalOptionRemark() {
        return element(by.id("oversightIntervalRemark_remark"));
    }

    getOversightCompleted() {
        return element(by.id("oversightCompleted"));
    }

    getOversightCompletedRemark() {
        return element(by.id("oversightCompletedRemark_remark"));
    }

    getOversightCompletedLatestDate() {
        return element(by.id("oversightCompletedLatestDate")).element(by.tagName("input"));
    }

}
export = DataProcessingRegistrationEditOversightPageObject;
