import IPageObject = require("../IPageObject.po");
import CSSLocatorHelper = require("../../object-wrappers/CSSLocatorHelper")
import KendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper");

var byDataElementType = new CSSLocatorHelper().byDataElementType;

class HomePagePo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl);
    }

    kendoToolbarWrapper = new KendoToolbarWrapper();

    public emailField = element(by.model("email"));
    public pwdField = element(by.model("password"));
    public loginButton = element(byDataElementType("loginButton"));

}

export = HomePagePo;