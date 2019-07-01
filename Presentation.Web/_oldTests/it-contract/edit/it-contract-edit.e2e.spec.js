"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../helper");
var ItContractEditPo = require("../../../app/components/it-contract/it-contract-edit.po");
describe("contract edit view", function () {
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
        "itInterfaceExhibitUsage",
        "interfaceUsage",
        "organization"
    ];
    beforeEach(function () {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);
        pageObject = new ItContractEditPo();
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
            expect(pageObject.nameElement).toBeDisabled();
            expect(pageObject.idElement).toBeDisabled();
            expect(pageObject.parentSelector.element).toBeSelect2Disabled();
            expect(pageObject.typeSelector.element).toBeSelect2Disabled();
            expect(pageObject.esdhElement).toBeDisabled();
            expect(pageObject.purchaseformSelector.element).toBeSelect2Disabled();
            expect(pageObject.strategySelector.element).toBeSelect2Disabled();
            expect(pageObject.planSelector.element).toBeSelect2Disabled();
            expect(pageObject.templateSelector.element).toBeSelect2Disabled();
            expect(pageObject.folderElement).toBeDisabled();
            expect(pageObject.supplierSelector.element).toBeSelect2Disabled();
            expect(pageObject.externSignerElement).toBeDisabled();
            expect(pageObject.orgUnitSelector.element).toBeSelect2Disabled();
            expect(pageObject.internSignerSelector.element).toBeSelect2Disabled();
            expect(pageObject.externSignCheckbox).toBeDisabled();
            expect(pageObject.externSignDateElement).toBeDisabled();
            expect(pageObject.internSignCheckbox).toBeDisabled();
            expect(pageObject.internSignDateElement).toBeDisabled();
            expect(pageObject.agreementElementsSelector.element).toBeSelect2Disabled();
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itContractWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should not delete when delete confirm popup is dismissed", function () {
            // arrange
            pageObject.deleteContractButton.first().click();
            // act
            browserHelper.dismissAlert();
            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itcontract/1" });
        });
        it("should delete when delete confirm popup is accepted", function () {
            // arrange
            pageObject.deleteContractButton.first().click();
            // act
            browserHelper.acceptAlert();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itcontract/1" });
        });
        it("should save when name looses focus", function () {
            // arrange
            // act
            pageObject.nameInput("SomeInput" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when id looses focus", function () {
            // arrange
            // act
            pageObject.idInput("SomeInput" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when parent is changed", function () {
            // arrange
            // act
            pageObject.parentSelector.selectFirst("c");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when type is changed", function () {
            // arrange
            // act
            pageObject.typeSelector.selectFirst();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when esdh looses focus", function () {
            // arrange
            // act
            pageObject.esdhInput("SomeInput" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when folder looses focus", function () {
            // arrange
            // act
            pageObject.folderInput("SomeInput" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when note looses focus", function () {
            // arrange
            // act
            pageObject.noteInput("SomeInput" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when purchaseform is changed", function () {
            // arrange
            // act
            pageObject.purchaseformSelector.selectFirst();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when strategy is changed", function () {
            // arrange
            // act
            pageObject.strategySelector.selectFirst();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when plan is changed", function () {
            // arrange
            // act
            pageObject.planSelector.selectFirst();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when template is changed", function () {
            // arrange
            // act
            pageObject.templateSelector.selectFirst();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when supplier is changed", function () {
            // arrange
            // act
            pageObject.supplierSelector.selectFirst("f");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when externSigner looses focus", function () {
            // arrange
            // act
            pageObject.externSignerInput("SomeInput" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when orgUnit is changed", function () {
            // arrange
            // act
            pageObject.orgUnitSelector.selectFirst();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when internSigner is changed", function () {
            // arrange
            // act
            pageObject.internSignerSelector.selectFirst("t");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when externSign checkbox is changed", function () {
            // arrange
            // act
            pageObject.externSignCheckbox.click();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when externSignDate looses focus", function () {
            // arrange
            // act
            pageObject.externSignDateInput("01-01-2016" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when internSign checkbox is changed", function () {
            // arrange
            // act
            pageObject.internSignCheckbox.click();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when internSignDate looses focus", function () {
            // arrange
            // act
            pageObject.internSignDateInput("01-01-2016" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        it("should save when agreementElements is changed", function () {
            // arrange
            // act
            pageObject.agreementElementsSelector.selectFirst("l");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "POST", url: "api/itcontract/1(.)+elemId=[0-9]+" });
        });
    });
});
