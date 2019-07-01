"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../helper");
var ItProjectEditPo = require("../../../app/components/it-project/it-project-edit.po");
describe("project edit view", function () {
    var mockHelper;
    var browserHelper;
    var pageObject;
    var mockDependencies = ["itproject", "itprojectrole", "itprojecttype", "itprojectright"];
    beforeEach(function () {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);
        pageObject = new ItProjectEditPo();
        browser.driver.manage().window().maximize();
    });
    afterEach(function () {
        mock.teardown();
    });
    describe("with no write access", function () {
        beforeEach(function (done) {
            mock(["itProjectNoWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should disable inputs", function () {
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
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should not delete when delete confirm popup is dismissed", function () {
            // arrange
            pageObject.deleteProjectElement.click();
            // act
            browserHelper.dismissAlert();
            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itproject/1" });
        });
        it("should delete when delete confirm popup is accepted", function () {
            // arrange
            pageObject.deleteProjectElement.click();
            // act
            browserHelper.acceptAlert();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itproject/1" });
        });
        it("should save when name looses focus", function () {
            // arrange
            pageObject.nameInput("SomeName");
            // act
            pageObject.idElement.click();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });
        it("should save when projectId looses focus", function () {
            // arrange
            pageObject.idInput("SomeId");
            // act
            pageObject.nameElement.click();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });
        it("should save when type is changed", function () {
            // arrange
            // act
            pageObject.typeSelect.selectFirst("lo");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });
        it("should save when cmdb looses focus", function () {
            // arrange
            pageObject.cmdbInput("SomeCmdb");
            // act
            pageObject.nameElement.click();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });
        it("should save when access is changed", function () {
            // arrange
            // act
            pageObject.accessSelect.selectFirst("p");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });
        it("should save when esdh looses focus", function () {
            // arrange
            pageObject.esdhInput("SomeEsdh");
            // act
            pageObject.nameElement.click();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });
        it("should save when folder looses focus", function () {
            // arrange
            pageObject.folderInput("SomeFolder");
            // act
            pageObject.nameElement.click();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });
        it("should save when background looses focus", function () {
            // arrange
            pageObject.backgroundInput("SomeBackground");
            // act
            pageObject.nameElement.click();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });
        it("should save when note looses focus", function () {
            // arrange
            pageObject.noteInput("SomeNote");
            // act
            pageObject.nameElement.click();
            // assert
            expect(mockHelper.lastRequest()).toBeTruthy({ method: "PATCH", url: "api/itproject/1" });
        });
        it("should save when archive checkbox is selected", function () {
            // arrange
            // act
            pageObject.archiveCheckbox.click();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });
        it("should save when transversal checkbox is selected", function () {
            // arrange
            // act
            pageObject.transversalCheckbox.click();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });
        it("should save when strategy checkbox is selected", function () {
            // arrange
            // act
            pageObject.strategyCheckbox.click();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });
        it("should search for projects when change in field", function () {
            // arrange
            // act
            pageObject.projectParentSelect.selectFirst("p");
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "GET", url: "api/itproject?(.*?)q=p" });
        });
    });
});
