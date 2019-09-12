import HomePage = require("../PageObjects/Organization/UsersPage.po");
import TestFixtureWrapper = require("../Utility/TestFixtureWrapper");
import Login = require("../Helpers/LoginHelper");
import WaitTimers = require("../Utility/waitTimers");

describe("Only Global and Local Admins can view API column in user overview", () => {
    var testFixture = new TestFixtureWrapper();
    var pageObject = new HomePage();
    var loginHelper = new Login();
    var waitUpTo = new WaitTimers();
    var ec = protractor.ExpectedConditions;

    beforeEach(() => {
       
    });

    afterEach(() => {
        testFixture.cleanupState();
    });

    it("Global Admin can see API access attribute in overview", () =>
    {
        loginHelper.loginAsGlobalAdmin();
        checkApiColumn(true);
    });

    it("Local Admin can see API access attribute in overview", () => {
        loginHelper.loginAsLocalAdmin();
        checkApiColumn(true);
    });

    it("Regular user cannot see API access attribute in overview", () =>
    {
        loginHelper.loginAsRegularUser();
        checkApiColumn(false);
    });

    function checkApiColumn(isColumnVisible : boolean)
    {
        pageObject.getPage();
        browser.wait(ec.presenceOf(pageObject.kendoToolbarWrapper.columnHeaders().userApi), waitUpTo.twentySeconds);

        if (isColumnVisible)
        {
            expect(pageObject.kendoToolbarWrapper.columnHeaders().userApi.isDisplayed()).toBeTruthy();
        }
        else
        {
            expect(pageObject.kendoToolbarWrapper.columnHeaders().userApi.isDisplayed()).toBeFalsy();
        }
    }

});