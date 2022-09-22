import UsersPage = require("../../PageObjects/Organization/UsersPage.po");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import Login = require("../../Helpers/LoginHelper");
import createUserHelper = require("../../Helpers/CreateUserHelper");
import WaitTimers = require("../../Utility/waitTimers");

describe("Only Global Admins can create user with special permissions, Parallel",
    () => {
        var testFixture = new TestFixtureWrapper();
        var pageObject = new UsersPage();
        var loginHelper = new Login();
        var waitUpTo = new WaitTimers();
        var ec = protractor.ExpectedConditions;
        var userHelper = new createUserHelper();

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

        it("It is possible to set primary start unit during user creation", () => {
            loginHelper
                .loginAsLocalAdmin()
                .then(() => pageObject.getPage())
                .then(() => browser.wait(ec.presenceOf(pageObject.createUserButton), waitUpTo.twentySeconds))
                .then(() => pageObject.createUserButton.click())
                .then(() => browser.waitForAngular())
                .then(() => userHelper.assertAllPrimaryStartUnitsAvailable());
        });
    });


