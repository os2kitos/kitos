"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../helper");
var ItInterfaceEditPo = require("../../../app/components/it-system/it-interface/it-interface-edit.po");
describe("system interface edit", function () {
    var mockHelper;
    var browserHelper;
    var pageObject;
    var mockDependencies = [
        "itInterface",
        "tsa",
        "interface",
        "interfaceType",
        "method",
        "datatype",
        "datarow",
        "organization"
    ];
    beforeEach(function () {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);
        pageObject = new ItInterfaceEditPo();
        browser.driver.manage().window().maximize();
    });
    afterEach(function () {
        mock.teardown();
    });
    describe("with no write access", function () {
        beforeEach(function (done) {
            mock(["itInterfaceNoWriteAccess"].concat(mockDependencies));
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
            expect(pageObject.versionElement).toBeDisabled();
            expect(pageObject.belongsToSelect.element).toBeSelect2Disabled();
            expect(pageObject.accessModifierSelect.element).toBeSelect2Disabled();
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itInterfaceWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should not delete when delete confirm popup is dismissed", function () {
            // arrange
            pageObject.deleteInterfaceElement.first().click();
            // act
            browserHelper.dismissAlert();
            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itinterface/1" });
        });
        it("should delete when delete confirm popup is accepted", function () {
            // arrange
            pageObject.deleteInterfaceElement.first().click();
            // act
            browserHelper.acceptAlert();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itinterface/1" });
        });
        it("should save when name looses focus", function () {
            // arrange
            // act
            pageObject.nameInput("SomeName" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });
        it("should save when id looses focus", function () {
            // arrange
            // act
            pageObject.idInput("SomeId" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });
        it("should save when version looses focus", function () {
            // arrange
            // act
            pageObject.versionInput("SomeVersion" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });
        it("should save when belongs to is changed", function () {
            // arrange
            // act
            pageObject.belongsToSelect.selectFirst("i");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });
        it("should save when access modifier is changed", function () {
            // arrange
            // act
            pageObject.accessModifierSelect.selectFirst("p");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });
        it("should save when description looses focus", function () {
            // arrange
            // act
            pageObject.descriptionInput("SomeDescription" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });
        it("should save when url looses focus", function () {
            // arrange
            // act
            pageObject.urlInput("SomeUrl" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });
        it("should save when note looses focus", function () {
            // arrange
            // act
            pageObject.versionInput("SomeNote" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });
    });
});
