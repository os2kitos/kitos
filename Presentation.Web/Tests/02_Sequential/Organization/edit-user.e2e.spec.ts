import HomePage = require("../../PageObjects/Organization/UsersPage.po");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import Login = require("../../Helpers/LoginHelper");
import WaitTimers = require("../../Utility/waitTimers");
import createUserHelper = require("../../Helpers/CreateUserHelper");

describe("Only Global Admins can edit user with special permissions, Sequential",
    () => {
        var testFixture = new TestFixtureWrapper();
        var userHelper = new createUserHelper();
        var pageObject = new HomePage();
        var loginHelper = new Login();
        var waitUpTo = new WaitTimers();
        var ec = protractor.ExpectedConditions;

        afterEach(() => {
            testFixture.cleanupState();
        });

        function openPage() {
            return loginHelper.loginAsGlobalAdmin()
                .then(() => pageObject.getPage())
                .then(() => browser.waitForAngular())
                .then(() => browser.wait(ec.presenceOf(pageObject.createUserButton), waitUpTo.twentySeconds));
        }

        function waitForKendoGridLoad() {
            return browser.waitForAngular()
                .then(() => browser.wait(ec.presenceOf(pageObject.kendoToolbarWrapper.columnHeaders().userEmail), waitUpTo.twentySeconds));
        }

        function executeSpecialPermissionUseCase(mutate: () => webdriver.promise.Promise<void>, validate: () => webdriver.promise.Promise<boolean>) {
            return openPage()
                .then(() => mutate())
                .then(() => waitForKendoGridLoad())
                .then(() => validate());
        }

        function canSetApiAccessTo(value: boolean) {
            const credentials = loginHelper.getLocalAdminCredentials(); //Modify local admin instance
            executeSpecialPermissionUseCase(() => {
                console.log("Updating API status to " + value);
                return userHelper.updateApiOnUser(credentials.username, value);
            },
                () => {
                    console.log("Checking that status is updated");
                    return userHelper.checkApiRoleStatusOnUser(credentials.username, value);
                });
        }

        function canSetStakeHolderAccessTo(value: boolean) {
            const credentials = loginHelper.getLocalAdminCredentials(); //Modify local admin instance
            executeSpecialPermissionUseCase(() => {
                console.log("Updating StakeHolderAccess status to " + value);
                return userHelper.updateStakeHolderAccessOnUser(credentials.username, value);
            },
                () => {
                    console.log("Checking that StakeHolderAccess status is updated");
                    return userHelper.checkStakeHolderAccessRoleStatusOnUser(credentials.username, value);
                });
        }

        it("Global admin is able to set api access to TRUE on existing user", () => {
            canSetApiAccessTo(true);
        });

        it("Global admin is able to set api access to FALSE on existing user", () => {
            canSetApiAccessTo(false);
        });

        it("Global admin is able to set StakeHolder access to TRUE on existing user", () => {
            canSetStakeHolderAccessTo(true);
        });

        it("Global admin is able to set StakeHolder access to FALSE on existing user", () => {
            canSetStakeHolderAccessTo(false);
        });

        it("It is possible to set primary start unit during user edit", () => {
            const credentials = loginHelper.getLocalAdminCredentials();
            openPage()
                .then(() => waitForKendoGridLoad())
                .then(() => userHelper.openEditUser(credentials.username))
                .then(() => userHelper.assertAllPrimaryStartUnitsAvailable());
        });
    });


