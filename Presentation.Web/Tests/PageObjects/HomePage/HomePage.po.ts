import IPageObject = require("../IPageObject.po");
import CSSLocatorHelper = require("../../object-wrappers/CSSLocatorHelper")

var byDataElementType = new CSSLocatorHelper().byDataElementType;

class HomePagePo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl);
    }

    public emailField = element(by.model("email"));
    public pwdField = element(by.model("password"));
    public loginButton = element(byDataElementType("loginButton"));

}

export = HomePagePo;