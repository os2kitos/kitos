import IPageObject = require("../IPageObject.po");
import CSSLocatorHelper = require("../../object-wrappers/CSSLocatorHelper")
import Constants = require("../../Utility/Constants");

class HomePagePo implements IPageObject {
    private ec = protractor.ExpectedConditions;
    private byDataElementType = new CSSLocatorHelper().byDataElementType;
    private consts = new Constants();

    public getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl);
    }

    public loginFormField = element(this.byDataElementType(this.consts.loginFormField));
    public emailField = element(by.model("email"));
    public pwdField = element(by.model("password"));
    public loginButton = element(this.byDataElementType("loginButton"));

    public isLoginAvailable(): webdriver.until.Condition<boolean> {
        return this.ec.visibilityOf(this.loginFormField);
    }
}

export = HomePagePo;