"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../helper");
var ItInterfaceEditTabDetailsPo = require("../../../app/components/it-system/it-interface/it-interface-edit-tab-details.po");
describe("system interface edit tab details", function () {
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
        "organization",
        "itSystem",
        "exhibit"
    ];
    beforeEach(function () {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);
        pageObject = new ItInterfaceEditTabDetailsPo();
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
            expect(pageObject.tsaSelector.element).toBeSelect2Disabled();
            expect(pageObject.typeSelector.element).toBeSelect2Disabled();
            expect(pageObject.interfaceSelector.element).toBeSelect2Disabled();
            expect(pageObject.methodSelector.element).toBeSelect2Disabled();
            expect(pageObject.exhibitSelector.element).toBeSelect2Disabled();
            pageObject.dataRowRepeater.each(function (el) {
                expect(el.element(pageObject.dataLocator)).toBeDisabled();
                expect(pageObject.dataTypeSelector.element).toBeSelect2Disabled();
            });
        });
        it("should hide delete and add data row buttons", function () {
            // arrange
            // act
            // assert
            pageObject.dataRowRepeater.each(function (el) {
                expect(el.element(pageObject.deleteLocator)).not.toBeVisible();
            });
            expect(pageObject.addDataButton).not.toBeVisible();
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itInterfaceWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should save when tsa to is changed", function () {
            // arrange
            // act
            pageObject.tsaSelector.selectFirst("n");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });
        it("should save when type is changed", function () {
            // arrange
            // act
            pageObject.typeSelector.selectFirst("p");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });
        it("should save when interface is changed", function () {
            // arrange
            // act
            pageObject.interfaceSelector.selectFirst("l");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });
        it("should save when method is changed", function () {
            // arrange
            // act
            pageObject.methodSelector.selectFirst("a");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });
        it("should save when exhibit is changed", function () {
            // arrange
            // act
            pageObject.exhibitSelector.selectFirst("i");
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/exhibit" });
        });
        it("should add new data row when add is clicked", function () {
            // arrange
            // act
            pageObject.addDataButton.click();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/dataRow" });
        });
        it("should delete data row when delete is clicked", function () {
            // arrange
            // act
            pageObject.dataRowRepeater
                .selectFirst(pageObject.deleteLocator)
                .first()
                .click();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/dataRow/1" });
        });
        it("should save when data looses focus", function () {
            // arrange
            // act
            pageObject.dataRowRepeater
                .selectFirst(pageObject.dataLocator)
                .first()
                .sendKeys("SomeDate" + protractor.Key.TAB);
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "PATCH", url: "api/dataRow/1" });
        });
        it("should save when data type is changed", function () {
            // arrange
            // act
            pageObject.dataTypeSelector.selectFirst();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "PATCH", url: "api/dataRow/1" });
        });
    });
});
