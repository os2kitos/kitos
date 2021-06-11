import HomePage = require("../PageObjects/Organization/UsersPage.po");
import TestFixtureWrapper = require("../Utility/TestFixtureWrapper");
import Login = require("../Helpers/LoginHelper");
import WaitTimers = require("../Utility/waitTimers");

describe("Special column access test", () => {
    var testFixture = new TestFixtureWrapper();
    var pageObject = new HomePage();
    var loginHelper = new Login();
    var waitUpTo = new WaitTimers();
    var ec = protractor.ExpectedConditions;

    afterEach(() => {
        testFixture.cleanupState();
    });

    it("Global Admin can see special attributes in overview", () => {
        loginHelper.loginAsGlobalAdmin();
        checkColumnVisibility(true, () => pageObject.kendoToolbarWrapper.columnHeaders().userApi, "api");
        checkColumnVisibility(true, () => pageObject.kendoToolbarWrapper.columnHeaders().userRightsHolderAccess, "RightsHolderAccess");
        checkColumnVisibility(true, () => pageObject.kendoToolbarWrapper.columnHeaders().userStakeHolderAccess, "StakeHolderAccess");
    });

    it("Local Admin can see API access but not RightsHolderAccess attribute in overview", () => {
        loginHelper.loginAsLocalAdmin();
        checkColumnVisibility(true, () => pageObject.kendoToolbarWrapper.columnHeaders().userApi, "api");
        checkColumnVisibility(false, () => pageObject.kendoToolbarWrapper.columnHeaders().userRightsHolderAccess, "RightsHolderAccess");
        checkColumnVisibility(false, () => pageObject.kendoToolbarWrapper.columnHeaders().userStakeHolderAccess, "StakeHolderAccess");
    });

    it("Regular user cannot see special attributes attribute in overview", () => {
        loginHelper.loginAsRegularUser();
        checkColumnVisibility(false, () => pageObject.kendoToolbarWrapper.columnHeaders().userApi, "api");
        checkColumnVisibility(false, () => pageObject.kendoToolbarWrapper.columnHeaders().userRightsHolderAccess, "RightsHolderAccess");
        checkColumnVisibility(false, () => pageObject.kendoToolbarWrapper.columnHeaders().userStakeHolderAccess, "StakeHolderAccess");
    });

    function checkColumnVisibility(isColumnVisible: boolean, getColumn: () => protractor.ElementFinder, logName: string) {
        console.log(`Testing that ${logName} column presence is ${isColumnVisible}`);

        pageObject.getPage();

        browser.wait(ec.presenceOf(pageObject.kendoToolbarWrapper.columnHeaders().userEmail), waitUpTo.twentySeconds);

        const expectation = expect(getColumn().getAttribute("style"));

        if (isColumnVisible) {
            //NOTE: Cannot use IsVisible method since it returns false if out of the visible area (scroll bars)
            expectation.not.toContain("display: none");
        }
        else {
            //NOTE: Cannot use IsVisible method since it returns false if out of the visible area (scroll bars)
            expectation.toContain("display: none");
        }
    }

});