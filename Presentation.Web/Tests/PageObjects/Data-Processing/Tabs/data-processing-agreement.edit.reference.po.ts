import PageObject = require("../../IPageObject.po");
import NavigationHelper = require("../../../Utility/NavigationHelper");
import CssLocatorHelper = require("../../../Object-wrappers/CSSLocatorHelper");

class DataProcessingAgreementEditReferencePageObject {
    private navigationHelper = new NavigationHelper();
    private cssHelper = new CssLocatorHelper();

    refreshPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.refreshPage();
    }

    getDpaReferenceTabButton() {
        return element(this.cssHelper.byDataElementType("ReferenceTabButton"));
    }

    getDpaReferenceCreateButton() {
        return element(this.cssHelper.byDataElementType("createReferenceButton"));
    }

}
export = DataProcessingAgreementEditReferencePageObject;