import login = require("../../Helpers/LoginHelper");
import HomePage = require("../../PageObjects/it-system/tabs/ItSystemReference.po");
import WaitUpTo = require("../../Utility/WaitTimers");
import ReferenceHelper = require("../../Helpers/ReferenceHelper");
import Constants = require("../../Utility/Constants");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");

var loginHelper = new login();
var homePage = new HomePage();
var waitUpTo = new WaitUpTo();
var refHelper = new ReferenceHelper();
var consts = new Constants();
var testFixture = new TestFixtureWrapper();

var headerButtons = homePage.kendoToolbarWrapper.headerButtons();
var inputFields = homePage.kendoToolbarWrapper.inputFields();
var colObjects = homePage.kendoToolbarWrapper.columnObjects();

describe("Can add and remove reference",
    () => {
        beforeAll(() => {
            loginHelper.loginAsLocalAdmin();
        });

        beforeEach(() => {
            homePage.getPage();
        });

        afterAll(() => {
            testFixture.cleanupState();
        });

        it("",
            () => {
                refHelper.createReference(consts.refTitle, consts.validUrl, consts.refId);
                expect(colObjects.referenceName.first().getText()).toEqual(consts.refTitle);
                refHelper.deleteReference(consts.refId);
                expect(colObjects.referenceName).toBeEmptyArray();
            });

    });

describe("Can edit reference URL",
    () => {

        beforeAll(() => {
            loginHelper.loginAsLocalAdmin();
            refHelper.createReference(consts.refTitle, consts.validUrl, consts.refId);
        });

        beforeEach(() => {
            homePage.getPage();
            browser.waitForAngular();
        });

        afterAll(() => {
            refHelper.deleteReference(consts.refId);
            testFixture.cleanupState();
        });


        it("Able to edit references", () => {
            browser.wait(homePage.isEditReferenceLoaded(), waitUpTo.twentySeconds);
            expect(headerButtons.editReference.isEnabled()).toBe(true);
        });

        it("Able to insert invalid url", () => {
            browser.wait(homePage.isEditReferenceLoaded(), waitUpTo.twentySeconds);
            headerButtons.editReference.click();
            inputFields.referenceDocUrl.clear();
            inputFields.referenceDocUrl.sendKeys(consts.invalidUrl);
            headerButtons.editSaveReference.click();

            expect(colObjects.referenceName.getAttribute("href")).not.toContain(consts.invalidUrl);
        });

        it("Able to insert valid url", () => {
            browser.wait(homePage.isEditReferenceLoaded(), waitUpTo.twentySeconds);
            headerButtons.editReference.click();
            inputFields.referenceDocUrl.clear();
            inputFields.referenceDocUrl.sendKeys(consts.validUrl);
            headerButtons.editSaveReference.click();
            expect(colObjects.referenceName.getAttribute("href")).toContain(consts.validUrl);
        });

    });