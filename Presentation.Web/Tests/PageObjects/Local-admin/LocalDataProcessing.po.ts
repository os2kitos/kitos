import PageObject = require("../IPageObject.po");
import NavigationHelper = require("../../Utility/NavigationHelper");
import constants = require("../../Utility/Constants");
import CssLocatorHelper = require("../../object-wrappers/CSSLocatorHelper");

class LocalDataProcessing implements PageObject {
    private navigationHelper = new NavigationHelper();
    private consts = new constants();
    private byDataElementType = new CssLocatorHelper().byDataElementType;

    getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/local-config/data-processing");
    }

    getToggleDataProcessingCheckbox() {
        return element(this.byDataElementType(this.consts.dataProcessingCheckbox)); 
    }
}

export = LocalDataProcessing;


