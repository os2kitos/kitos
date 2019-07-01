"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../helper");
var ItSystemEditPo = require("../../../app/components/it-system/edit/it-system-edit.po");
describe("system edit view", function () {
    var mockHelper;
    var browserHelper;
    var pageObject;
    var mockDependencies = [
        "itSystem",
        "businessType",
        "itSystemTypeOption",
        "itInterfaceUse",
        "organization"
    ];
    beforeEach(function () {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);
        pageObject = new ItSystemEditPo();
        browser.driver.manage().window().maximize();
    });
    afterEach(function () {
        mock.teardown();
    });
    describe("with no write access", function () {
        beforeEach(function (done) {
            mock(["itSystemNoWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should disable inputs", function () {
            // arrange
            // act
            // assert
            expect(pageObject.appTypeSelect.element).toBeSelect2Disabled();
            expect(pageObject.nameElement).toBeDisabled();
            expect(pageObject.systemParentSelect.element).toBeSelect2Disabled();
            expect(pageObject.belongsToSelect.element).toBeSelect2Disabled();
            expect(pageObject.accessModifierSelect.element).toBeSelect2Disabled();
            expect(pageObject.usageTypeSelector.element).toBeSelect2Disabled();
            expect(pageObject.descriptionElement).toBeDisabled();
            expect(pageObject.furtherDescriptionElement).toBeDisabled();
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itSystemWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should not delete when delete confirm popup is dismissed", function () {
            // arrange
            pageObject.deleteSystemElement.click();
            // act
            browserHelper.dismissAlert();
            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itsystem/1" });
        });
        it("should delete when delete confirm popup is accepted", function () {
            // arrange
            pageObject.deleteSystemElement.click();
            // act
            browserHelper.acceptAlert();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itsystem/1" });
        });
        it("should save when appType changes", function () {
            // arrange
            // act
            pageObject.appTypeSelect.selectFirst("i");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        });
        it("should save when name looses focus", function () {
            // arrange
            // act
            pageObject.nameInput("SomeName");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        });
        it("should save when system parent is changed", function () {
            // arrange
            // act
            pageObject.systemParentSelect.selectFirst("i");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        });
        it("should save when belongs to is changed", function () {
            // arrange
            // act
            pageObject.belongsToSelect.selectFirst("i");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        });
        it("should save when access modifier is changed", function () {
            // arrange
            // act
            pageObject.accessModifierSelect.selectFirst("p");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        });
        it("should save when usage type is changed", function () {
            // arrange
            // act
            pageObject.usageTypeSelector.selectFirst("p");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        });
        it("should save when description looses focus", function () {
            // arrange
            // act
            pageObject.descriptionInput("SomeDescription");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        });
        it("should save when further description looses focus", function () {
            // arrange
            // act
            pageObject.furtherDescriptionInput("SomeFurtherDescription");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        });
    });
});
