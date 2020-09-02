import PageObject = require("../IPageObject.po");
import NavigationHelper = require("../../Utility/NavigationHelper");
import constants = require("../../Utility/Constants");
import CssLocatorHelper = require("../../object-wrappers/CSSLocatorHelper");

class LocalProject implements PageObject {
    private navigationHelper = new NavigationHelper();
    private ec = protractor.ExpectedConditions;
    private consts = new constants();
    private byDataElementType = new CssLocatorHelper().byDataElementType;

    getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/local-config/DataProcessingAgreement");
    }

    dpaCheckbox = element(this.byDataElementType(this.consts.dataProcessingAgreementChecbox));

    getToggleDataProcessingAgreementCheckbox() {
        return this.dpaCheckbox; 
    }

    waitForDataProcessingAgreementCheckboxVisibility(): webdriver.until.Condition<boolean> {
        return this.ec.visibilityOf(this.dpaCheckbox);
    }
}

export = LocalProject;


