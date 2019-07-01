"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../../helper");
var ItContractEditTabDeadlinesPo = require("../../../../app/components/it-contract/tabs/it-contract-tab-deadlines.po");
describe("contract edit tab deadlines", function () {
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
        "optionExtend",
        "terminationDeadline",
        "paymentMilestone",
        "handoverTrialType",
        "handoverTrial",
        "organizationunit"
    ];
    beforeEach(function () {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);
        pageObject = new ItContractEditTabDeadlinesPo();
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
            pageObject.handoverTrialsRepeater.each(function (row) {
                expect(pageObject.handoverTrialLocator.element).toBeSelect2Disabled();
                expect(row.element(pageObject.handoverTrialExpectedDateLocator)).toBeDisabled();
                expect(row.element(pageObject.handoverTrialApprovedDateLocator)).toBeDisabled();
            });
            pageObject.paymentMilestonesRepeater.each(function (row) {
                expect(row.element(pageObject.paymentMilestoneLocator)).toBeDisabled();
                expect(row.element(pageObject.paymentMilestoneExpectedDateLocator)).toBeDisabled();
                expect(row.element(pageObject.paymentMilestoneApprovedDateLocator)).toBeDisabled();
            });
            expect(pageObject.agreementConcludedElement).toBeDisabled();
            expect(pageObject.agreementDurationSelector.element).toBeSelect2Disabled();
            expect(pageObject.agreementOptionExtendSelector.element).toBeSelect2Disabled();
            expect(pageObject.agreementOptionExtendMultiplierElement).toBeDisabled();
            expect(pageObject.agreementIrrevocableElement).toBeDisabled();
            expect(pageObject.agreementExpirationElement).toBeDisabled();
            expect(pageObject.agreementTerminatedElement).toBeDisabled();
            expect(pageObject.agreementNoticeSelector.element).toBeSelect2Disabled();
        });
        it("should hide inputs", function () {
            // assert
            expect(pageObject.handoverTrialSelector.element).not.toBeVisible();
            expect(pageObject.handoverTrialExpectedDateElement).not.toBeVisible();
            expect(pageObject.handoverTrialApprovedDateElement).not.toBeVisible();
            expect(pageObject.paymentMilestoneElement).not.toBeVisible();
            expect(pageObject.paymentMilestoneExpectedDateElement).not.toBeVisible();
            expect(pageObject.paymentMilestoneApprovedDateElement).not.toBeVisible();
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itContractWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        xit("should save when handoverTrial is changed", function () {
            // arrange
            // act
            pageObject.handoverTrialLocator.selectFirst("d");
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/handoverTrial/1" });
        });
        xit("should save when handoverTrialExpectedDate looses focus", function () {
            // arrange
            // act
            pageObject.handoverTrialsRepeater
                .selectFirst(pageObject.handoverTrialExpectedDateLocator)
                .first()
                .sendKeys("01-02-2016" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/handoverTrial/1" });
        });
        xit("should save when handoverTrialApprovedDate looses focus", function () {
            // arrange
            // act
            pageObject.handoverTrialsRepeater
                .selectFirst(pageObject.handoverTrialApprovedDateLocator)
                .first()
                .sendKeys("01-02-2016" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/handoverTrial/1" });
        });
        xit("should save handover trial when save is clicked", function () {
            // arrange
            pageObject.handoverTrialSelector.selectFirst();
            pageObject.handoverTrialExpectedDateInput("01-01-2016");
            pageObject.handoverTrialApprovedDateInput("01-01-2016");
            // act
            pageObject.handoverTrialAddButton.click();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/handoverTrial" });
        });
        xit("should delete handover trial when delete is clicked", function () {
            // arrange
            // act
            pageObject.handoverTrialsRepeater
                .selectFirst(pageObject.handoverTrialDeleteLocator)
                .first()
                .click();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/handoverTrial/1" });
        });
        xit("should save when paymentMilestone looses focus", function () {
            // arrange
            // act
            pageObject.paymentMilestonesRepeater
                .selectFirst(pageObject.paymentMilestoneLocator)
                .first()
                .sendKeys("SomeInput" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/paymentMilestone/1" });
        });
        xit("should save when paymentMilestoneExpectedDate looses focus", function () {
            // arrange
            // act
            pageObject.paymentMilestonesRepeater
                .selectFirst(pageObject.paymentMilestoneExpectedDateLocator)
                .first()
                .sendKeys("01-02-2016" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/paymentMilestone/1" });
        });
        xit("should save when paymentMilestoneApprovedDate looses focus", function () {
            // arrange
            // act
            pageObject.paymentMilestonesRepeater
                .selectFirst(pageObject.paymentMilestoneApprovedDateLocator)
                .first()
                .sendKeys("01-02-2016" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/paymentMilestone/1" });
        });
        xit("should save handover trial when save is clicked", function () {
            // arrange
            pageObject.paymentMilestoneInput("SomeInput");
            pageObject.paymentMilestoneExpectedDateInput("01-01-2016");
            pageObject.paymentMilestoneApprovedDateInput("01-01-2016");
            // act
            pageObject.paymentMilestoneAddButton.click();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/paymentMilestone" });
        });
        xit("should delete handover trial when delete is clicked", function () {
            // arrange
            // act
            pageObject.paymentMilestonesRepeater
                .selectFirst(pageObject.paymentMilestoneDeleteLocator)
                .first()
                .click();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/paymentMilestone/1" });
        });
        xit("should save when agreementConcluded looses focus", function () {
            // arrange
            // act
            //pageObject.agreementConcludedInput(`01-02-2016${protractor.Key.TAB}`);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        xit("should save when agreementDuration is changed", function () {
            // arrange
            // act
            pageObject.agreementDurationSelector.selectFirst();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        xit("should save when agreementOptionExtend is changed", function () {
            // arrange
            // act
            pageObject.agreementOptionExtendSelector.selectFirst();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        xit("should save when agreementOptionExtendMultiplier looses focus", function () {
            // arrange
            // act
            pageObject.agreementOptionExtendMultiplierInput("1" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        xit("should save when agreementIrrevocable looses focus", function () {
            // arrange
            // act
            pageObject.agreementIrrevocableInput("01-02-2016" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        xit("should save when agreementExpiration looses focus", function () {
            // arrange
            // act
            pageObject.agreementExpirationInput("01-02-2016" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        xit("should save when agreementTerminated looses focus", function () {
            // arrange
            // act
            pageObject.agreementTerminatedInput("01-02-2016" + protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
        xit("should save when agreementNotice is changed", function () {
            // arrange
            // act
            pageObject.agreementNoticeSelector.selectFirst();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
    });
});
