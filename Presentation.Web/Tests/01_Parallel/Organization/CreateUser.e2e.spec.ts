import HomePage = require("../../PageObjects/Organization/UsersPage.po");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import Login = require("../../Helpers/LoginHelper");
import WaitTimers = require("../../Utility/waitTimers");

describe("Only Global Admins can create user with special permissions, Parallel",
    () => {
        var testFixture = new TestFixtureWrapper();
        var pageObject = new HomePage();
        var loginHelper = new Login();
        var waitUpTo = new WaitTimers();
        var ec = protractor.ExpectedConditions;

        afterEach(() => {
            testFixture.cleanupState();
        });

        it("Global Admin can enable special permissions on new user", () => {
            loginHelper.loginAsGlobalAdmin();
            pageObject.getPage();
            browser.wait(ec.presenceOf(pageObject.createUserButton), waitUpTo.twentySeconds);
            pageObject.createUserButton.click();
            expect(pageObject.hasAPiCheckBox.isDisplayed()).toBeTrue();
            expect(pageObject.hasRightsHolderAccessCheckBox.isDisplayed()).toBeTrue();
            expect(pageObject.hasStakeHolderAccessCheckBox.isPresent()).toBeTrue();
        });

        it("Local Admin cannot enable special permissions on new user", () => {
            loginHelper.loginAsLocalAdmin();
            pageObject.getPage();
            browser.wait(ec.presenceOf(pageObject.createUserButton), waitUpTo.twentySeconds);
            pageObject.createUserButton.click();
            expect(pageObject.hasAPiCheckBox.isPresent()).toBeFalse();
            expect(pageObject.hasRightsHolderAccessCheckBox.isPresent()).toBeFalse();
            expect(pageObject.hasStakeHolderAccessCheckBox.isPresent()).toBeFalse();
        });
    });


