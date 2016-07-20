import mock = require("protractor-http-mock");
import Helper = require("../../helper");
import ItProjectEditPo = require("../../../app/components/it-project/it-project-edit.po");

describe("project edit view", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: ItProjectEditPo;
    var mockDependencies: Array<string> = ["itproject", "itprojectrole", "itprojecttype", "itprojectright"];

    beforeEach(() => {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);
        pageObject = new ItProjectEditPo();
        browser.driver.manage().window().maximize();
    });

    afterEach(() => {
        mock.teardown();
    });

    describe("with no write access", () => {
        beforeEach(done => {
            mock(["itProjectNoWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should disable inputs", () => {
            // arrange

            // act

            // assert
            expect(pageObject.nameElement).toBeDisabled();
            expect(pageObject.idElement).toBeDisabled();
            expect(pageObject.typeSelect.element).toBeSelect2Disabled();
            expect(pageObject.cmdbElement).toBeDisabled();
            expect(pageObject.accessSelect.element).toBeSelect2Disabled();
            expect(pageObject.esdhElement).toBeDisabled();
            expect(pageObject.folderElement).toBeDisabled();
            expect(pageObject.backgroundElement).toBeDisabled();
            expect(pageObject.noteElement).toBeDisabled();
            expect(pageObject.archiveCheckbox).toBeDisabled();
            expect(pageObject.transversalCheckbox).toBeDisabled();
            expect(pageObject.strategyCheckbox).toBeDisabled();
            expect(pageObject.projectParentSelect.element).toBeSelect2Disabled();
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should not delete when delete confirm popup is dismissed", () => {
            // arrange
            pageObject.deleteProjectElement.click();

            // act
            browserHelper.dismissAlert();

            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itproject/1" });
        });

        it("should delete when delete confirm popup is accepted", () => {
            // arrange
            pageObject.deleteProjectElement.click();

            // act
            browserHelper.acceptAlert();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itproject/1" });
        });

        it("should save when name looses focus", () => {
            // arrange
            pageObject.nameInput("SomeName");

            // act
            pageObject.idElement.click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should save when projectId looses focus", () => {
            // arrange
            pageObject.idInput("SomeId");

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
            pageObject.cmdbInput("SomeCmdb");

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
            pageObject.esdhInput("SomeEsdh");

            // act
            pageObject.nameElement.click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should save when folder looses focus", () => {
            // arrange
            pageObject.folderInput("SomeFolder");

            // act
            pageObject.nameElement.click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should save when background looses focus", () => {
            // arrange
            pageObject.backgroundInput("SomeBackground");

            // act
            pageObject.nameElement.click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should save when note looses focus", () => {
            // arrange
            pageObject.noteInput("SomeNote");

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
