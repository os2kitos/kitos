import PageObject = require("../IPageObject.po");
import NavigationHelper = require("../../Utility/NavigationHelper");
import constants = require("../../Utility/Constants");

class LocalProject implements PageObject {
    private navigationHelper = new NavigationHelper();

    getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/local-config/DataProcessorAgreement");
    }
}

export = LocalProject;