import IPageObject = require("../IPageObject.po");
import CSSLocatorHelper = require("../../object-wrappers/CSSLocatorHelper")
import KendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper");
var ec = protractor.ExpectedConditions;
var byDataElementType = new CSSLocatorHelper().byDataElementType;

class HomePagePo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl);
    }

    public loginFormField = new KendoToolbarWrapper().FieldsForms().loginForm;

    public emailField = element(by.model("email"));
    public pwdField = element(by.model("password"));
    public loginButton = element(byDataElementType("loginButton"));


    public isLoginAvailable(): webdriver.until.Condition<boolean> {
        return ec.visibilityOf(this.loginFormField);
    }
}

export = HomePagePo;