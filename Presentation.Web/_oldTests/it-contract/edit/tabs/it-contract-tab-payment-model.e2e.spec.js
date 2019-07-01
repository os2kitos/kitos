"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../../helper");
var ItContractEditTabPaymentModelPo = require("../../../../app/components/it-contract/tabs/it-contract-tab-paymentmodel.po");
describe("contract edit tab payment model", function () {
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
        "paymentFrequency",
        "paymentModel",
        "priceRegulation",
        "organizationunit"
    ];
    beforeEach(function () {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);
        pageObject = new ItContractEditTabPaymentModelPo();
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
            // assert
            expect(pageObject.operationDateElement).toBeDisabled();
            expect(pageObject.freqSelector.element).toBeSelect2Disabled();
            expect(pageObject.paymentSelector.element).toBeSelect2Disabled();
            expect(pageObject.priceSelector.element).toBeSelect2Disabled();
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itContractWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should save when operationDate looses focus", function () {
            // arrange
            // act
            pageObject.operationDateInput("01-01-2016" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when freq is changed", function () {
            // arrange
            // act
            pageObject.freqSelector.selectFirst();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when payment is changed", function () {
            // arrange
            // act
            pageObject.paymentSelector.selectFirst();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when price is changed", function () {
            // arrange
            // act
            pageObject.priceSelector.selectFirst();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
    });
});
