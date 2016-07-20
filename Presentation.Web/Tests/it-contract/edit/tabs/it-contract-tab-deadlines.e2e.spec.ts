import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import ItContractEditTabDeadlinesPo = require("../../../../app/components/it-contract/tabs/it-contract-tab-deadlines.po");

describe("contract edit tab deadlines", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: ItContractEditTabDeadlinesPo;
    var mockDependencies: Array<string> = [
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

    beforeEach(() => {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);
        pageObject = new ItContractEditTabDeadlinesPo();
        browser.driver.manage().window().maximize();
    });

    afterEach(() => {
        mock.teardown();
    });

    describe("with no write access", () => {
        beforeEach(done => {
            mock(["itContractNoWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should disable inputs", () => {
            // assert
            pageObject.handoverTrialsRepeater.each(row => {
                expect(pageObject.handoverTrialLocator.element).toBeSelect2Disabled();
                expect(row.element(pageObject.handoverTrialExpectedDateLocator)).toBeDisabled();
                expect(row.element(pageObject.handoverTrialApprovedDateLocator)).toBeDisabled();
            });

            pageObject.paymentMilestonesRepeater.each(row => {
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

        it("should hide inputs", () => {
            // assert
            expect(pageObject.handoverTrialSelector.element).not.toBeVisible();
            expect(pageObject.handoverTrialExpectedDateElement).not.toBeVisible();
            expect(pageObject.handoverTrialApprovedDateElement).not.toBeVisible();

            expect(pageObject.paymentMilestoneElement).not.toBeVisible();
            expect(pageObject.paymentMilestoneExpectedDateElement).not.toBeVisible();
            expect(pageObject.paymentMilestoneApprovedDateElement).not.toBeVisible();
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itContractWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        xit("should save when handoverTrial is changed", () => {
            // arrange

            // act
            pageObject.handoverTrialLocator.selectFirst("d");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/handoverTrial/1" });
        });

        xit("should save when handoverTrialExpectedDate looses focus", () => {
            // arrange

            // act
            pageObject.handoverTrialsRepeater
                .selectFirst(pageObject.handoverTrialExpectedDateLocator)
                .first()
                .sendKeys(`01-02-2016${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/handoverTrial/1" });
        });

        xit("should save when handoverTrialApprovedDate looses focus", () => {
            // arrange

            // act
            pageObject.handoverTrialsRepeater
                .selectFirst(pageObject.handoverTrialApprovedDateLocator)
                .first()
                .sendKeys(`01-02-2016${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/handoverTrial/1" });
        });

        xit("should save handover trial when save is clicked", () => {
            // arrange
            pageObject.handoverTrialSelector.selectFirst();
            pageObject.handoverTrialExpectedDateInput("01-01-2016");
            pageObject.handoverTrialApprovedDateInput("01-01-2016");

            // act
            pageObject.handoverTrialAddButton.click();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/handoverTrial" });
        });

        xit("should delete handover trial when delete is clicked", () => {
            // arrange

            // act
            pageObject.handoverTrialsRepeater
                .selectFirst(pageObject.handoverTrialDeleteLocator)
                .first()
                .click();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/handoverTrial/1" });
        });

        xit("should save when paymentMilestone looses focus", () => {
            // arrange

            // act
            pageObject.paymentMilestonesRepeater
                .selectFirst(pageObject.paymentMilestoneLocator)
                .first()
                .sendKeys(`SomeInput${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/paymentMilestone/1" });
        });

        xit("should save when paymentMilestoneExpectedDate looses focus", () => {
            // arrange

            // act
            pageObject.paymentMilestonesRepeater
                .selectFirst(pageObject.paymentMilestoneExpectedDateLocator)
                .first()
                .sendKeys(`01-02-2016${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/paymentMilestone/1" });
        });

        xit("should save when paymentMilestoneApprovedDate looses focus", () => {
            // arrange

            // act
            pageObject.paymentMilestonesRepeater
                .selectFirst(pageObject.paymentMilestoneApprovedDateLocator)
                .first()
                .sendKeys(`01-02-2016${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/paymentMilestone/1" });
        });

        xit("should save handover trial when save is clicked", () => {
            // arrange
            pageObject.paymentMilestoneInput("SomeInput");
            pageObject.paymentMilestoneExpectedDateInput("01-01-2016");
            pageObject.paymentMilestoneApprovedDateInput("01-01-2016");

            // act
            pageObject.paymentMilestoneAddButton.click();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/paymentMilestone" });
        });

        xit("should delete handover trial when delete is clicked", () => {
            // arrange

            // act
            pageObject.paymentMilestonesRepeater
                .selectFirst(pageObject.paymentMilestoneDeleteLocator)
                .first()
                .click();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/paymentMilestone/1" });
        });

        xit("should save when agreementConcluded looses focus", () => {
            // arrange

            // act
            //pageObject.agreementConcludedInput(`01-02-2016${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        xit("should save when agreementDuration is changed", () => {
            // arrange

            // act
            pageObject.agreementDurationSelector.selectFirst();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        xit("should save when agreementOptionExtend is changed", () => {
            // arrange

            // act
            pageObject.agreementOptionExtendSelector.selectFirst();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        xit("should save when agreementOptionExtendMultiplier looses focus", () => {
            // arrange

            // act
            pageObject.agreementOptionExtendMultiplierInput(`1${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        xit("should save when agreementIrrevocable looses focus", () => {
            // arrange

            // act
            pageObject.agreementIrrevocableInput(`01-02-2016${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        xit("should save when agreementExpiration looses focus", () => {
            // arrange

            // act
            pageObject.agreementExpirationInput(`01-02-2016${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        xit("should save when agreementTerminated looses focus", () => {
            // arrange

            // act
            pageObject.agreementTerminatedInput(`01-02-2016${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        xit("should save when agreementNotice is changed", () => {
            // arrange

            // act
            pageObject.agreementNoticeSelector.selectFirst();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
    });
});
