import PageObject = require("../IPageObject.po");
import NavigationHelper = require("../../Utility/NavigationHelper");
import constants = require("../../Utility/Constants");

class LocalProject implements PageObject {
    private navigationHelper = new NavigationHelper();
    private static consts = new constants();

    getPage(): webdriver.promise.Promise<void> {
        return this.navigationHelper.getPage("/#/local-config/project");
    }

    static getIncludeModuleInputElement() {
        return element(by.id(this.consts.itProjectIncludeModuleInput));
    }
}

export = LocalProject;