import mock = require("protractor-http-mock");
import Helper = require("../../helper");
import ItProjectEditPo = require("../../../app/components/it-project/it-project-edit.po");

describe("project edit view", () => {
    var browserHelper: Helper.Browser;
    var mockHelper: Helper.Mock;
    var pageObject: ItProjectEditPo;

    beforeEach(() => {
        mock(["itProjectWriteAccess", "itproject", "itprojectrole", "itprojecttype", "itprojectrights"]);

        browserHelper = new Helper.Browser(browser);
        mockHelper = new Helper.Mock();

        pageObject = new ItProjectEditPo();
        pageObject.getPage();

        browser.driver.manage().window().maximize();

        // clear initial requests
        mock.clearRequests();
    });

    afterEach(() => {
        mock.teardown();
    });

    it("should not delete when delete confirm popup is dismissed", () => {
        // arrange
        pageObject.deleteProjectElement.first().click();

        // act
        browser.switchTo().alert()
            .then(alert => alert.dismiss());

        // assert
        expect(mockHelper.findRequest({ method: "DELETE", url: "api/itproject/1" })).toBeFalsy();
    });

    it("should delete when delete confirm popup is accepted", () => {
        // arrange
        pageObject.deleteProjectElement.first().click();

        // act
        browser.switchTo().alert()
            .then(alert => alert.accept());

        // assert
        expect(mockHelper.findRequest({ method: "DELETE", url: "api/itproject/1" })).toBeTruthy();
    });

    it("should save when name looses focus", () => {
        // arrange
        pageObject.nameInput = "SomeName";

        // act
        pageObject.idElement.click();

        // assert
        expect(mockHelper.lastRequest({ method: "PATCH", url: "api/itproject/1" })).toBeTruthy();
    });

    it("should save when projectId looses focus", () => {
        // arrange
        pageObject.idInput = "SomeId";

        // act
        pageObject.nameElement.click();

        // assert
        expect(mockHelper.lastRequest({ method: "PATCH", url: "api/itproject/1" })).toBeTruthy();
    });

    it("should save when type is changed", () => {
        // arrange

        // act
        pageObject.typeSelect.selectFirst("lo");

        // assert
        expect(mockHelper.lastRequest({ method: "PATCH", url: "api/itproject/1" })).toBeTruthy();
    });

    it("should save when cmdb looses focus", () => {
        // arrange
        pageObject.cmdbInput = "SomeCmdb";

        // act
        pageObject.nameElement.click();

        // assert
        expect(mockHelper.lastRequest({ method: "PATCH", url: "api/itproject/1" })).toBeTruthy();
    });
    it("should save when access is changed", () => {
        // arrange

        // act
        pageObject.accessSelect.selectFirst("p");

        // assert
        expect(mockHelper.lastRequest({ method: "PATCH", url: "api/itproject/1" })).toBeTruthy();
    });

    it("should save when projectEsdh looses focus", () => {
        // arrange
        pageObject.esdhInput = "SomeEsdh";

        // act
        pageObject.nameElement.click();

        // assert
        expect(mockHelper.lastRequest({ method: "PATCH", url: "api/itproject/1" })).toBeTruthy();
    });

    it("should save when projectFolder looses focus", () => {
        // arrange
        pageObject.folderInput = "SomeFolder";

        // act
        pageObject.nameElement.click();

        // assert
        expect(mockHelper.lastRequest({ method: "PATCH", url: "api/itproject/1" })).toBeTruthy();
    });

    it("should save when background looses focus", () => {
        // arrange
        pageObject.backgroundInput = "SomeBackground";

        // act
        pageObject.nameElement.click();

        // assert
        expect(mockHelper.lastRequest({ method: "PATCH", url: "api/itproject/1" })).toBeTruthy();
    });

    it("should save when note looses focus", () => {
        // arrange
        pageObject.noteInput = "SomeNote";

        // act
        pageObject.nameElement.click();

        // assert
        expect(mockHelper.lastRequest({ method: "PATCH", url: "api/itproject/1" })).toBeTruthy();
    });

    it("should save when archive checkbox is selected", () => {
        // arrange

        // act
        pageObject.archiveCheckbox.click();

        // assert
        expect(mockHelper.lastRequest({ method: "PATCH", url: "api/itproject/1" })).toBeTruthy();
    });

    it("should save when transversal checkbox is selected", () => {
        // arrange

        // act
        pageObject.transversalCheckbox.click();

        // assert
        expect(mockHelper.lastRequest({ method: "PATCH", url: "api/itproject/1" })).toBeTruthy();
    });

    it("should save when strategy checkbox is selected", () => {
        // arrange

        // act
        pageObject.strategyCheckbox.click();

        // assert
        expect(mockHelper.lastRequest({ method: "PATCH", url: "api/itproject/1" })).toBeTruthy();
    });

    it("should search for projects when change in field", () => {
        // arrange

        // act
        pageObject.projectParentSelect.selectFirst("p");

        // assert
        expect(mockHelper.findRequest({ method: "GET", url: "api/itproject?(.*?)q=p" })).toBeTruthy();
    });
});
