﻿import login = require("../Helpers/LoginHelper");
import loginPage = require("../PageObjects/HomePage/LoginPage.po")
import testFixtureWrapper = require("../Utility/TestFixtureWrapper");
import waitTimers = require("../Utility/waitTimers");

var pageObject = new loginPage();
var navigationBarHelper = pageObject.navigationBarHelper;
var navigationBar = pageObject.navigationBar;
var testFixture = new testFixtureWrapper();
var loginHelper = new login();
var wait = new waitTimers();
var ec = protractor.ExpectedConditions;

describe("Being logged out, it is possible to login ", () => { 

    beforeEach(() => {
       // testFixture.disableAutoBrowserWaits();
    });

    afterEach(() => {
        testFixture.enableAutoBrowserWaits();
        testFixture.cleanupState();
    });

    it("As global admin", () => {
        loginHelper.loginAsGlobalAdmin();
        browser.sleep(wait.oneSecond);
        browser.wait(ec.visibilityOf(navigationBar.dropDownMenu.dropDownElement));
        navigationBarHelper.dropDownExpand();
        expect(navigationBarHelper.isMyProfileDisplayed()).toBeTruthy();
        expect(navigationBarHelper.isGlobalAdminDisplayed()).toBeTruthy();
        expect(navigationBarHelper.isLocalAdminDisplayed()).toBeFalsy();
    });

    it("As local admin", () => {
        loginHelper.loginAsLocalAdmin();
        browser.sleep(wait.oneSecond);
        browser.wait(ec.visibilityOf(navigationBar.dropDownMenu.dropDownElement));
        navigationBarHelper.dropDownExpand();
        expect(navigationBarHelper.isMyProfileDisplayed()).toBeTruthy();
        expect(navigationBarHelper.isGlobalAdminDisplayed()).toBeFalsy();
        expect(navigationBarHelper.isLocalAdminDisplayed()).toBeTruthy();
    });

    it("As regular user", () => {
        loginHelper.loginAsRegularUser();
        browser.sleep(wait.oneSecond);
        browser.wait(ec.visibilityOf(navigationBar.dropDownMenu.dropDownElement));
        navigationBarHelper.dropDownExpand();
        expect(navigationBarHelper.isMyProfileDisplayed()).toBeTruthy();
        expect(navigationBarHelper.isGlobalAdminDisplayed()).toBeFalsy();
        expect(navigationBarHelper.isLocalAdminDisplayed()).toBeFalsy();
    });

});