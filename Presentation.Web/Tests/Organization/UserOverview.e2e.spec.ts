import HomePage = require("../PageObjects/Organization/UsersPage.po");
import TestFixtureWrapper = require("../Utility/TestFixtureWrapper");
import Login = require("../Helpers/LoginHelper");
import WaitTimers = require("../Utility/waitTimers");

var testFixture = new TestFixtureWrapper();
var pageObject = new HomePage();
var loginHelper = new Login();
var waitUpTo = new WaitTimers();
var ec = protractor.ExpectedConditions;

describe("Only Global and Local Admins can view API column in user overview", () => {

    beforeEach(() => {
       
    });

    afterEach(() => {
        testFixture.cleanupState();
    });

    it("Global Admin can see API access attribute in overview", () =>
    {
        loginHelper.loginAsGlobalAdmin();
        pageObject.getPage();
        browser.wait(ec.presenceOf(pageObject.kendoToolbarWrapper.columnHeaders().userApi), waitUpTo.twentySeconds);
        expect(pageObject.kendoToolbarWrapper.columnHeaders().userApi.isDisplayed()).toBeTruthy();
    });

    it("Local Admin can see API access attribute in overview", () => {
        loginHelper.loginAsLocalAdmin();
        pageObject.getPage();
        browser.wait(ec.presenceOf(pageObject.kendoToolbarWrapper.columnHeaders().userApi), waitUpTo.twentySeconds);
        expect(pageObject.kendoToolbarWrapper.columnHeaders().userApi.isDisplayed()).toBeTruthy();
    });

    it("Regular user cannot see API access attribute in overview", () =>
    {
        loginHelper.loginAsRegularUser();
        pageObject.getPage();
        browser.wait(ec.presenceOf(pageObject.kendoToolbarWrapper.columnHeaders().userApi), waitUpTo.twentySeconds);
        expect(pageObject.kendoToolbarWrapper.columnHeaders().userApi.isDisplayed()).toBeFalsy();
    });

});