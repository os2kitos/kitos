import Helper = require("../helper");
import ItSystemEditPo = require("../PageObjects/it-system/it-system-overview.po");

describe("Regular user tests", () => {

    var loginHelper = new Helper.Login();
    var pageObject = new ItSystemEditPo();

    beforeAll(() => {
        loginHelper.login("support@kitos.dk", "testpwrd"); //TODO change user info.
        browser.waitForAngular();
    });

    beforeEach(() => {
        pageObject.getPage();
        browser.waitForAngular();
    });

    it("Use filter is disabled", () => {
        //Arrange

        //Act 

        //Assert
        expect(pageObject.useFiltersButton.getAttribute("disabled")).toEqual("true");
    });

    it("Delete filter is disabled", () => {
        //Arrange

        //Act 

        //Assert
        expect(pageObject.useFiltersButton.getAttribute("disabled")).toEqual("true");
    });

    it("Delete filter is disabled", () => {
        //Arrange

        //Act 

        //Assert
        expect(pageObject.exportButton.getText()).toEqual("Eksportér til Excel");
    });

    it("Delete filter is disabled", () => {
        //Arrange
        //Act 
        //Assert
        expect(pageObject.columnHeaderValidity.getText()).toEqual("Gyldig/Ikke gyldig");
    });


});