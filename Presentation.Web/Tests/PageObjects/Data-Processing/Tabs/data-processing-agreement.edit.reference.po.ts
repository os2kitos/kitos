import PageObject = require("../../IPageObject.po");
import NavigationHelper = require("../../../Utility/NavigationHelper");
import CssLocatorHelper = require("../../../Object-wrappers/CSSLocatorHelper");
import DataProcessingAgreementNavigation = require("../../../Helpers/SideNavigation/DataProcessingAgreementNavigation");

class DataProcessingAgreementEditReferencePageObject {
    private navigationHelper = new NavigationHelper();

    private cssHelper = new CssLocatorHelper();

    refreshPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.refreshPage();
    }

    goToDpaReferenceTab() {
        return DataProcessingAgreementNavigation.referencePage();
    }

    getDpaReferenceCreateButton() {
        return element(this.cssHelper.byDataElementType("createReferenceButton"));
    }

}
export = DataProcessingAgreementEditReferencePageObject;