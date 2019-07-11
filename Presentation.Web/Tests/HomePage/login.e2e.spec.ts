import login = require("../Helpers/LoginHelper");
import loginPage = require("../PageObjects/HomePage/LoginPage.po")

var pageObject = new loginPage;
var navigationBar = pageObject.navigationBarHelper;
var loginHelper = new login();

describe("Being logged out, it is possible to login ", () => { 

    beforeEach(() => {

    });

    afterEach(() => {
        browser.driver.manage().deleteAllCookies();
    });

    it("As global admin", () => {
        loginHelper.loginAsGlobalAdmin();
        navigationBar.dropDownExpand();
        expect(navigationBar.isMyProfileDisplayed()).toBeTruthy();
        expect(navigationBar.isGlobalAdminDisplayed()).toBeTruthy();
        expect(navigationBar.isLocalAdminDisplayed()).toBeFalsy();
    });

    it("As local admin", () => {
        loginHelper.loginAsLocalAdmin();
        navigationBar.dropDownExpand();
        expect(navigationBar.isMyProfileDisplayed()).toBeTruthy();
        expect(navigationBar.isGlobalAdminDisplayed()).toBeFalsy();
        expect(navigationBar.isLocalAdminDisplayed()).toBeTruthy();
    });

    it("As regular user", () => {
        loginHelper.loginAsRegularUser();
        navigationBar.dropDownExpand();
        expect(navigationBar.isMyProfileDisplayed()).toBeTruthy();
        expect(navigationBar.isGlobalAdminDisplayed()).toBeFalsy();
        expect(navigationBar.isLocalAdminDisplayed()).toBeFalsy();
    });

});