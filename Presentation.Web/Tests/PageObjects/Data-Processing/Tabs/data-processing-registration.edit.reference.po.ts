import PageObject = require("../../IPageObject.po");
import NavigationHelper = require("../../../Utility/NavigationHelper");
import CssLocatorHelper = require("../../../Object-wrappers/CSSLocatorHelper");
import DataProcessingRegistrationNavigation = require("../../../Helpers/SideNavigation/DataProcessingRegistrationNavigation");

class DataProcessingRegistrationEditReferencePageObject {
    private navigationHelper = new NavigationHelper();

    private cssHelper = new CssLocatorHelper();

    refreshPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.refreshPage();
    }

    goToDpaReferenceTab() {
        return DataProcessingRegistrationNavigation.referencePage();
    }

    getDpaReferenceCreateButton() {
        return element(this.cssHelper.byDataElementType("createReferenceButton"));
    }

}
export = DataProcessingRegistrationEditReferencePageObject;
