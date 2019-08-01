import HomePage = require("../PageObjects/Organization/UsersPage.po");
import TestFixtureWrapper = require("../Utility/TestFixtureWrapper");
import Login = require("../Helpers/LoginHelper");
import WaitTimers = require("../Utility/waitTimers");

var testFixture = new TestFixtureWrapper();
var pageObject = new HomePage();
var loginHelper = new Login();
var waitUpTo = new WaitTimers();
var ec = protractor.ExpectedConditions;

describe("Only Global Admins can enable and disable API access on a user", () => {

    beforeEach(() => {
       
    });

    afterEach(() => {
        testFixture.cleanupState();
        
    });

    it("Global Admin only have access to API access attribute in overview", () =>
    {
        loginHelper.loginAsGlobalAdmin();
        pageObject.getPage();
        expect(pageObject.kendoToolbarWrapper.columnHeaders().userApi.isPresent()).toBeTruthy();
    });

    it("Local Admin and down cannot see API access attribute in overview", () =>
    {
        loginHelper.loginAsLocalAdmin();
        pageObject.getPage();
        expect(pageObject.kendoToolbarWrapper.columnHeaders().userApi.isPresent()).toBeFalsy();
    });

});