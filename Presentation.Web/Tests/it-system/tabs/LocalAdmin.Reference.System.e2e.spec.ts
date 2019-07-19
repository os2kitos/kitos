import login = require("../../Helpers/LoginHelper");
import homePage = require("../../PageObjects/it-system/tabs/ItSystemReference.po");
import WaitUpTo = require("../../Utility/WaitTimers");
import Constants = require("../../Utility/Constants");

describe("Local admin can edit reference URL", () => {

    var loginHelper = new login();
    var pageObject = new homePage();
    var waitUpTo = new WaitUpTo();
    var consts = new Constants();

    var headerButtons = pageObject.kendoToolbarWrapper.headerButtons();
    var inputFields = pageObject.kendoToolbarWrapper.inputFields();
    var colObjects = pageObject.kendoToolbarWrapper.columnObjects();
    

    beforeAll(() => {
        loginHelper.loginAsLocalAdmin();
    });

    beforeEach(() => {
        pageObject.getPage();
        browser.waitForAngular();
    });

    it("Able to edit references", () => {
        browser.wait(pageObject.isReferenceLoaded(), waitUpTo.twentySeconds);
        expect(headerButtons.editReference.isEnabled()).toBe(true);
    });

    it("Able to insert invalid url", () => {
        browser.wait(pageObject.isReferenceLoaded(), waitUpTo.twentySeconds);
        headerButtons.editReference.click();
        inputFields.referenceDocUrl.clear();
        inputFields.referenceDocUrl.sendKeys(consts.invalidUrl);
        headerButtons.editSaveReference.click();

        expect(colObjects.referenceName.getAttribute("href")).not.toContain(consts.invalidUrl);
    });

    it("Able to insert valid url", () => {
        browser.wait(pageObject.isReferenceLoaded(), waitUpTo.twentySeconds);
        headerButtons.editReference.click();
        inputFields.referenceDocUrl.clear();
        inputFields.referenceDocUrl.sendKeys(consts.validUrl);
        headerButtons.editSaveReference.click();

       expect(colObjects.referenceName.getAttribute("href")).toContain(consts.validUrl);

    });

});