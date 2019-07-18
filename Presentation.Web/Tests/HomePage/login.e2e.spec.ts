import Login = require("../Helpers/LoginHelper");
import LoginPage = require("../PageObjects/HomePage/LoginPage.po")
import TestFixtureWrapper = require("../Utility/TestFixtureWrapper");
import WaitTimers = require("../Utility/waitTimers");

var pageObject = new LoginPage();
var navigationBarHelper = pageObject.navigationBarHelper;
var navigationBar = pageObject.navigationBar;
var testFixture = new TestFixtureWrapper();
var loginHelper = new Login();
var waitUpTo = new WaitTimers();
var ec = protractor.ExpectedConditions;

describe("Being logged out, it is possible to login ", () => { 

    beforeEach(() => {
        testFixture.disableAutoBrowserWaits();
    });

    afterEach(() => {
        testFixture.enableAutoBrowserWaits();
        testFixture.cleanupState();
    });

    it("As global admin", () => {
        loginHelper.loginAsGlobalAdmin();
        browser.wait(ec.visibilityOf(navigationBar.dropDownMenu.dropDownElement), waitUpTo.twentySeconds);
        navigationBarHelper.dropDownExpand();
        expect(navigationBarHelper.isMyProfileDisplayed()).toBeTruthy();
        expect(navigationBarHelper.isGlobalAdminDisplayed()).toBeTruthy();
        expect(navigationBarHelper.isLocalAdminDisplayed()).toBeFalsy();
    });

    it("As local admin", () => {
        loginHelper.loginAsLocalAdmin();
        browser.wait(ec.visibilityOf(navigationBar.dropDownMenu.dropDownElement), waitUpTo.twentySeconds);
        navigationBarHelper.dropDownExpand();
        expect(navigationBarHelper.isMyProfileDisplayed()).toBeTruthy();
        expect(navigationBarHelper.isGlobalAdminDisplayed()).toBeFalsy();
        expect(navigationBarHelper.isLocalAdminDisplayed()).toBeTruthy();
    });

    it("As regular user", () => {
        loginHelper.loginAsRegularUser();
        browser.wait(ec.visibilityOf(navigationBar.dropDownMenu.dropDownElement), waitUpTo.twentySeconds);
        navigationBarHelper.dropDownExpand();
        expect(navigationBarHelper.isMyProfileDisplayed()).toBeTruthy();
        expect(navigationBarHelper.isGlobalAdminDisplayed()).toBeFalsy();
        expect(navigationBarHelper.isLocalAdminDisplayed()).toBeFalsy();
    });

});