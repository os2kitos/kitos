import login = require("../../Helpers/LoginHelper");
import ItSystemEditPo = require("../../PageObjects/it-system/tabs/ItSystemReference.po");

describe("Local admin tests", () => {

    var EC = protractor.ExpectedConditions;
    var loginHelper = new login();
    var pageObject = new ItSystemEditPo();
    var headerButtons = pageObject.kendoToolbarWrapper.headerButtons();
    var inputFields = pageObject.kendoToolbarWrapper.inputFields();
    var colObjects = pageObject.kendoToolbarWrapper.columnObjects();
    var headerButtonsHelper = pageObject.kendoToolbarHelper.headerButtons;
    var validUrl = "https://strongminds.dk/";

    beforeAll(() => {
        loginHelper.loginAsLocalAdmin();
        browser.waitForAngular();
    });

    beforeEach(() => {
        pageObject.getPage();
        browser.waitForAngular();
    });

    it("Able to edit references", () => {

        expect(headerButtons.editReference.isEnabled()).toBe(true);

    });

    it("Able to insert invalid url", () => {

        headerButtons.editReference.click();

        inputFields.referenceDocUrl.clear();

        inputFields.referenceDocUrl.sendKeys("invalidurl.com");

        headerButtons.editSaveReference.click();

        expect(colObjects.referenceName.get(0).isPresent()).toBeFalsy();

    });

    it("Able to insert valid url", () => {

        headerButtons.editReference.click();

        inputFields.referenceDocUrl.clear();

        inputFields.referenceDocUrl.sendKeys(validUrl);

        headerButtons.editSaveReference.click();


        expect(colObjects.referenceName.get(0).getAttribute("href")).toEqual(validUrl);


    });

});