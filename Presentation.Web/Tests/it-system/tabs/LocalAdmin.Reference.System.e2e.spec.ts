import login = require("../../Helpers/LoginHelper");
import HomePage = require("../../PageObjects/it-system/tabs/ItSystemReference.po");
import WaitUpTo = require("../../Utility/WaitTimers");
import ReferenceHelper = require("../../Helpers/ReferenceHelper");
import Constants = require("../../Utility/Constants");

describe("Local admin can edit reference URL",
    () => {

        var loginHelper = new login();
        var homePage = new HomePage();
        var waitUpTo = new WaitUpTo();
        var refHelper = new ReferenceHelper();
        var consts = new Constants();

        var headerButtons = homePage.kendoToolbarWrapper.headerButtons();
        var inputFields = homePage.kendoToolbarWrapper.inputFields();
        var colObjects = homePage.kendoToolbarWrapper.columnObjects();


        beforeAll(() => {
            loginHelper.loginAsLocalAdmin();
            browser.waitForAngular();
            refHelper.createReference(consts.refTitle, consts.validUrl, consts.refId);

        });

        beforeEach(() => {
            homePage.getPage();
            browser.waitForAngular();
        });

        afterAll(() => {
            refHelper.deleteReference(consts.refId);
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