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

    getOversightPage() {
        DataProcessingRegistrationNavigation.oversightPage();
    }
    
    getOversightIntervalOption() {
        return element(by.id("oversightInterval"));
    }
    getOversightIntervalOptionNote() {
        return element(this.cssHelper.byDataElementType("oversightIntervalNote"));
    }

}
export = DataProcessingRegistrationEditMainPageObject;
