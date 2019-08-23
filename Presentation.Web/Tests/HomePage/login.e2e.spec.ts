import Login = require("../Helpers/LoginHelper");
import LoginPage = require("../PageObjects/HomePage/LoginPage.po");
import TestFixtureWrapper = require("../Utility/TestFixtureWrapper");

describe("Being logged out, it is possible to login ", () => { 
    var pageObject = new LoginPage();
    var navigationBarHelper = pageObject.navigationBarHelper;
    var testFixture = new TestFixtureWrapper();
    var loginHelper = new Login();

    beforeEach(() => {
        testFixture.disableAutoBrowserWaits();
    });

    afterEach(() => {
        testFixture.enableAutoBrowserWaits();
        testFixture.cleanupState();
    });

    it("As global admin", () => {
        loginHelper.loginAsGlobalAdmin();
        navigationBarHelper.dropDownExpand();
        expect(navigationBarHelper.isMyProfileDisplayed()).toBeTruthy();
        expect(navigationBarHelper.isGlobalAdminDisplayed()).toBeTruthy();
        expect(navigationBarHelper.isLocalAdminDisplayed()).toBeFalsy();
    });

    it("As local admin", () => {
        loginHelper.loginAsLocalAdmin();
        navigationBarHelper.dropDownExpand();
        expect(navigationBarHelper.isMyProfileDisplayed()).toBeTruthy();
        expect(navigationBarHelper.isGlobalAdminDisplayed()).toBeFalsy();
        expect(navigationBarHelper.isLocalAdminDisplayed()).toBeTruthy();
    });

    it("As regular user", () => {
        loginHelper.loginAsRegularUser();
        navigationBarHelper.dropDownExpand();
        expect(navigationBarHelper.isMyProfileDisplayed()).toBeTruthy();
        expect(navigationBarHelper.isGlobalAdminDisplayed()).toBeFalsy();
        expect(navigationBarHelper.isLocalAdminDisplayed()).toBeFalsy();
    });

});