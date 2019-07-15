import login = require("../Helpers/LoginHelper");
import loginPage = require("../PageObjects/HomePage/LoginPage.po")

var pageObject = new loginPage;
var navigationBarHelper = pageObject.navigationBarHelper;
var navigationBar = pageObject.navigationBar;
var loginHelper = new login();

var ec = protractor.ExpectedConditions;

describe("Being logged out, it is possible to login ", () => { 

    beforeEach(() => {
        browser.ignoreSynchronization = true;
    });

    afterEach(() => {
        browser.ignoreSynchronization = false;
        browser.driver.manage().deleteAllCookies();
    });

    it("As global admin", () => {
        loginHelper.loginAsGlobalAdmin();
        browser.wait(ec.visibilityOf(navigationBar.dropDownMenu.dropDownElement));
        navigationBarHelper.dropDownExpand();
        expect(navigationBarHelper.isMyProfileDisplayed()).toBeTruthy();
        expect(navigationBarHelper.isGlobalAdminDisplayed()).toBeTruthy();
        expect(navigationBarHelper.isLocalAdminDisplayed()).toBeFalsy();
    });

    it("As local admin", () => {
        loginHelper.loginAsLocalAdmin();
        browser.wait(ec.visibilityOf(navigationBar.dropDownMenu.dropDownElement));
        navigationBarHelper.dropDownExpand();
        expect(navigationBarHelper.isMyProfileDisplayed()).toBeTruthy();
        expect(navigationBarHelper.isGlobalAdminDisplayed()).toBeFalsy();
        expect(navigationBarHelper.isLocalAdminDisplayed()).toBeTruthy();
    });

    it("As regular user", () => {
        loginHelper.loginAsRegularUser();
        browser.wait(ec.visibilityOf(navigationBar.dropDownMenu.dropDownElement));
        navigationBarHelper.dropDownExpand();
        expect(navigationBarHelper.isMyProfileDisplayed()).toBeTruthy();
        expect(navigationBarHelper.isGlobalAdminDisplayed()).toBeFalsy();
        expect(navigationBarHelper.isLocalAdminDisplayed()).toBeFalsy();
    });

});