"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../../helper");
var ItContractEditTabEconomyPo = require("../../../../app/components/it-contract/tabs/it-contract-tab-economy.po");
describe("contract edit tab economy", function () {
    var mockHelper;
    var browserHelper;
    var pageObject;
    var mockDependencies = [
        "itContract",
        "contractType",
        "contractTemplate",
        "purchaseForm",
        "procurementStrategy",
        "agreementElement",
        "organizationunit",
        "economyStream"
    ];
    beforeEach(function () {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);
        pageObject = new ItContractEditTabEconomyPo();
        browser.driver.manage().window().maximize();
    });
    afterEach(function () {
        mock.teardown();
    });
    describe("with no write access", function () {
        beforeEach(function (done) {
            mock(["itContractNoWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should disable inputs", function () {
            // arrange
            // act
            // assert
            pageObject.externRepeater.each(function (row) {
                expect(pageObject.externOrgUnitSelector.element).toBeSelect2Disabled();
                expect(row.element(pageObject.externAcquisitionLocator)).toBeDisabled();
                expect(row.element(pageObject.externOperationLocator)).toBeDisabled();
                expect(row.element(pageObject.externOtherLocator)).toBeDisabled();
                expect(row.element(pageObject.externAccountingEntryLocator)).toBeDisabled();
                expect(pageObject.externAuditStatus.isDisabled()).toBeTruthy("extern status selector should be disabled");
            });
            pageObject.internRepeater.each(function (row) {
                expect(pageObject.internOrgUnitSelector.element).toBeSelect2Disabled();
                expect(row.element(pageObject.internAcquisitionLocator)).toBeDisabled();
                expect(row.element(pageObject.internOperationLocator)).toBeDisabled();
                expect(row.element(pageObject.internOtherLocator)).toBeDisabled();
                expect(row.element(pageObject.internAccountingEntryLocator)).toBeDisabled();
                expect(pageObject.internAuditStatus.isDisabled()).toBeTruthy("intern status selector should be disabled");
            });
        });
        it("should hide new stream buttons", function () {
            // arrange
            // act
            // assert
            expect(pageObject.newExternButton).not.toBeVisible();
            expect(pageObject.newInternButton).not.toBeVisible();
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itContractWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should repeat extern streams", function () {
            // arrange
            // act
            // assert
            expect(pageObject.externRepeater.count()).toBeGreaterThan(0);
        });
        it("should save when extern org unit is changed", function () {
            // arrange
            // act
            pageObject.externRepeater.each(function () {
                pageObject.externOrgUnitSelector.selectFirst();
            });
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/economyStream/1" });
        });
        it("should save when extern acquisition looses focus", function () {
            // arrange
            // act
            pageObject.externRepeater
                .selectFirst(pageObject.externAcquisitionLocator)
                .first()
                .sendKeys("2" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/economyStream/1" });
        });
        it("should save when extern operation looses focus", function () {
            // arrange
            // act
            pageObject.externRepeater
                .selectFirst(pageObject.externOperationLocator)
                .first()
                .sendKeys("2" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/economyStream/1" });
        });
        it("should save when extern other looses focus", function () {
            // arrange
            // act
            pageObject.externRepeater
                .selectFirst(pageObject.externOtherLocator)
                .first()
                .sendKeys("2" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/economyStream/1" });
        });
        it("should save when extern accounting entry looses focus", function () {
            // arrange
            // act
            pageObject.externRepeater
                .selectFirst(pageObject.externAccountingEntryLocator)
                .first()
                .sendKeys("2" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/economyStream/1" });
        });
        it("should save when extern audit status is changed", function () {
            // arrange
            // act
            pageObject.externAuditStatus.selectFirst();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/economyStream/1" });
        });
        it("should save when extern audi date looses focus", function () {
            // arrange
            // act
            pageObject.externRepeater
                .selectFirst(pageObject.externAuditDateLocator)
                .first()
                .sendKeys("2" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/economyStream/1" });
        });
        it("should save when extern note looses focus", function () {
            // arrange
            // act
            pageObject.externRepeater
                .selectFirst(pageObject.externNoteLocator)
                .first()
                .sendKeys("2" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/economyStream/1" });
        });
        it("should not delete extern stream when delete confirm popup is dismissed", function () {
            // arrange
            pageObject.externRepeater
                .selectFirst(pageObject.externDeleteLocator)
                .first()
                .click();
            // act
            browserHelper.dismissAlert();
            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/economyStream/1" });
        });
        it("should delete extern stream when delete confirm popup is accepted", function () {
            // arrange
            pageObject.externRepeater
                .selectFirst(pageObject.externDeleteLocator)
                .first()
                .click();
            // act
            browserHelper.acceptAlert();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/economyStream/1" });
        });
        it("should repeat intern streams", function () {
            // arrange
            // act
            // assert
            expect(pageObject.internRepeater.count()).toBeGreaterThan(0);
        });
        it("should save when intern org unit is changed", function () {
            // arrange
            // act
            pageObject.internRepeater.each(function () {
                pageObject.internOrgUnitSelector.selectFirst();
            });
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/economyStream/2" });
        });
        it("should save when intern acquisition looses focus", function () {
            // arrange
            // act
            pageObject.internRepeater
                .selectFirst(pageObject.internAcquisitionLocator)
                .first()
                .sendKeys("2" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/economyStream/2" });
        });
        it("should save when intern operation looses focus", function () {
            // arrange
            // act
            pageObject.internRepeater
                .selectFirst(pageObject.internOperationLocator)
                .first()
                .sendKeys("2" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/economyStream/2" });
        });
        it("should save when intern other looses focus", function () {
            // arrange
            // act
            pageObject.internRepeater
                .selectFirst(pageObject.internOtherLocator)
                .first()
                .sendKeys("2" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/economyStream/2" });
        });
        it("should save when intern accounting entry looses focus", function () {
            // arrange
            // act
            pageObject.internRepeater
                .selectFirst(pageObject.internAccountingEntryLocator)
                .first()
                .sendKeys("2" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/economyStream/2" });
        });
        it("should save when intern audit status is changed", function () {
            // arrange
            // act
            pageObject.internAuditStatus.selectFirst();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/economyStream/2" });
        });
        it("should save when intern audi date looses focus", function () {
            // arrange
            // act
            pageObject.internRepeater
                .selectFirst(pageObject.internAuditDateLocator)
                .first()
                .sendKeys("2" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/economyStream/2" });
        });
        it("should save when intern note looses focus", function () {
            // arrange
            // act
            pageObject.internRepeater
                .selectFirst(pageObject.internNoteLocator)
                .first()
                .sendKeys("2" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/economyStream/2" });
        });
        it("should not delete intern stream when delete confirm popup is dismissed", function () {
            // arrange
            pageObject.internRepeater
                .selectFirst(pageObject.internDeleteLocator)
                .first()
                .click();
            // act
            browserHelper.dismissAlert();
            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/economyStream/2" });
        });
        it("should delete intern stream when delete confirm popup is accepted", function () {
            // arrange
            pageObject.internRepeater
                .selectFirst(pageObject.internDeleteLocator)
                .first()
                .click();
            // act
            browserHelper.acceptAlert();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/economyStream/2" });
        });
    });
});
