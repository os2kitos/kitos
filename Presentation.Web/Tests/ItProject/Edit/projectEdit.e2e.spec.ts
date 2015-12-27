import mock = require("protractor-http-mock");
import Helper = require("../../helper");
import ItProjectEditPo = require("../../../app/components/it-project/it-project-edit.po");

describe("project edit view", () => {
    var mockHelper: Helper.Mock;
    var pageObject: ItProjectEditPo;
    var mockDependencies: Array<string> = ["itproject", "itprojectrole", "itprojecttype", "itprojectrights"];

    beforeEach(() => {
        mockHelper = new Helper.Mock();

        pageObject = new ItProjectEditPo();

        browser.driver.manage().window().maximize();
    });

    afterEach(() => {
        mock.teardown();
    });

    describe("with no write access", () => {
        beforeEach(() => {
            mock(["itProjectNoWriteAccess"].concat(mockDependencies));
            pageObject.getPage();

            // clear initial requests
            mock.clearRequests();
        });

        it("should disable name", () => {
            // arrange

            // act

            // assert
            expect(pageObject.nameElement).toBeDisabled();
        });

        it("should disable projectId", () => {
            // arrange

            // act

            // assert
            expect(pageObject.idElement).toBeDisabled();
        });

        it("should disable type", () => {
            // arrange

            // act

            // assert
            expect(pageObject.typeSelect.isDisabled()).toBeTrue();
        });

        it("should disable cmdb", () => {
            // arrange

            // act

            // assert
            expect(pageObject.cmdbElement).toBeDisabled();
        });

        it("should disable access", () => {
            // arrange

            // act

            // assert
            expect(pageObject.accessSelect.isDisabled()).toBeTrue();
        });

        it("should disable esdh", () => {
            // arrange

            // act

            // assert
            expect(pageObject.esdhElement).toBeDisabled();
        });

        it("should disable folder", () => {
            // arrange

            // act

            // assert
            expect(pageObject.folderElement).toBeDisabled();
        });

        it("should disable background", () => {
            // arrange

            // act

            // assert
            expect(pageObject.backgroundElement).toBeDisabled();
        });

        it("should disable note", () => {
            // arrange

            // act

            // assert
            expect(pageObject.noteElement).toBeDisabled();
        });

        it("should disable archive checkbox", () => {
            // arrange

            // act

            // assert
            expect(pageObject.archiveCheckbox).toBeDisabled();
        });

        it("should disable transversal checkbox", () => {
            // arrange

            // act

            // assert
            expect(pageObject.transversalCheckbox).toBeDisabled();
        });

        it("should disable strategy checkbox", () => {
            // arrange

            // act

            // assert
            expect(pageObject.strategyCheckbox).toBeDisabled();
        });

        it("should disable project parent", () => {
            // arrange

            // act

            // assert
            expect(pageObject.projectParentSelect.element).toBeSelect2Disabled();
        });
    });

    describe("with write access", () => {
        beforeEach(() => {
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage();

            // clear initial requests
            mock.clearRequests();
        });

        it("should not delete when delete confirm popup is dismissed", () => {
            // arrange
            pageObject.deleteProjectElement.first().click();

            // act
            browser.switchTo().alert()
                .then(alert => alert.dismiss());

            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itproject/1" });
        });

        it("should delete when delete confirm popup is accepted", () => {
            // arrange
            pageObject.deleteProjectElement.first().click();

            // act
            browser.switchTo().alert()
                .then(alert => alert.accept());

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itproject/1" });
        });

        it("should save when name looses focus", () => {
            // arrange
            pageObject.nameInput = "SomeName";

            // act
            pageObject.idElement.click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should save when projectId looses focus", () => {
            // arrange
            pageObject.idInput = "SomeId";

            // act
            pageObject.nameElement.click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should save when type is changed", () => {
            // arrange

            // act
            pageObject.typeSelect.selectFirst("lo");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should save when cmdb looses focus", () => {
            // arrange
            pageObject.cmdbInput = "SomeCmdb";

            // act
            pageObject.nameElement.click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should save when access is changed", () => {
            // arrange

            // act
            pageObject.accessSelect.selectFirst("p");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should save when esdh looses focus", () => {
            // arrange
            pageObject.esdhInput = "SomeEsdh";

            // act
            pageObject.nameElement.click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should save when folder looses focus", () => {
            // arrange
            pageObject.folderInput = "SomeFolder";

            // act
            pageObject.nameElement.click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should save when background looses focus", () => {
            // arrange
            pageObject.backgroundInput = "SomeBackground";

            // act
            pageObject.nameElement.click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should save when note looses focus", () => {
            // arrange
            pageObject.noteInput = "SomeNote";

            // act
            pageObject.nameElement.click();

            // assert
            expect(mockHelper.lastRequest()).toBeTruthy({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should save when archive checkbox is selected", () => {
            // arrange

            // act
            pageObject.archiveCheckbox.click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should save when transversal checkbox is selected", () => {
            // arrange

            // act
            pageObject.transversalCheckbox.click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should save when strategy checkbox is selected", () => {
            // arrange

            // act
            pageObject.strategyCheckbox.click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should search for projects when change in field", () => {
            // arrange

            // act
            pageObject.projectParentSelect.selectFirst("p");

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "GET", url: "api/itproject?(.*?)q=p" });
        });
    });
});
