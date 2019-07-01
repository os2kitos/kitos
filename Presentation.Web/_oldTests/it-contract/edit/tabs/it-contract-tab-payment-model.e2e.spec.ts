import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import ItContractEditTabPaymentModelPo = require("../../../../app/components/it-contract/tabs/it-contract-tab-paymentmodel.po");

describe("contract edit tab payment model", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: ItContractEditTabPaymentModelPo;
    var mockDependencies: Array<string> = [
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

    beforeEach(() => {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);

        pageObject = new ItContractEditTabPaymentModelPo();

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
            expect(pageObject.operationDateElement).toBeDisabled();
            expect(pageObject.freqSelector.element).toBeSelect2Disabled();
            expect(pageObject.paymentSelector.element).toBeSelect2Disabled();
            expect(pageObject.priceSelector.element).toBeSelect2Disabled();
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itContractWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should save when operationDate looses focus", () => {
            // arrange

            // act
            pageObject.operationDateInput(`01-01-2016${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when freq is changed", () => {
            // arrange

            // act
            pageObject.freqSelector.selectFirst();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when payment is changed", () => {
            // arrange

            // act
            pageObject.paymentSelector.selectFirst();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when price is changed", () => {
            // arrange

            // act
            pageObject.priceSelector.selectFirst();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });
    });
});
