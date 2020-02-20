import IPageObject = require("../IPageObject.po");
import CSSLocatorHelper = require("../../object-wrappers/CSSLocatorHelper")
import Constants = require("../../Utility/Constants");
import SelectHelper = require("../../Helpers/SelectHelper");

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
    public selectWorkingOrganizationDialog = element(this.byDataElementType("selectWorkingOrganizationDialog"));
    public selectWorkingOrganizationButton = element(this.byDataElementType("selectWorkingOrganizationButton"));

    public isLoginAvailable(): webdriver.until.Condition<boolean> {
        return this.ec.visibilityOf(this.loginFormField);
    }

    public selectDefaultOrganizationAsWorkingOrg() {
        return SelectHelper.openAndSelect("selectWorkingOrganizationOptions","Fælles Kommune");
    }

    public selectSpecificOrganizationAsWorkingOrg(org: string) {
        return SelectHelper.openAndSelect("selectWorkingOrganizationOptions", org);
    }
}
export = HomePagePo;