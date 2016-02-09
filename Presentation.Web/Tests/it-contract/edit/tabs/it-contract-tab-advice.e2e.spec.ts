import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import ItContractEditTabAdvicePo = require("../../../../app/components/it-contract/tabs/it-contract-tab-advice.po");

describe("contract edit tab advice", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: ItContractEditTabAdvicePo;
    var mockDependencies: Array<string> = [
        "itContract",
        "contractType",
        "contractTemplate",
        "purchaseForm",
        "procurementStrategy",
        "agreementElement",
        "itContractRole",
        "organizationunit",
        "advice"
    ];

    beforeEach(() => {
        browser.driver.manage().window().maximize();

        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);
        pageObject = new ItContractEditTabAdvicePo();
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

        it("should hide new advice button", () => {
            // arrange

            // act

            // assert
            expect(pageObject.addButton).not.toBeVisible();
        });

        it("should hide delete buttons on advices", () => {
            // arrange

            // act

            // assert
            expect(element.all(pageObject.deleteButtonLocator).first()).not.toBeVisible();
        });

        it("should disable inputs", () => {
            // arrange

            // act

            // assert
            expect(element.all(pageObject.activeLocator).first()).toBeDisabled();
            expect(element.all(pageObject.nameLocator).first()).toBeDisabled();
            expect(element.all(pageObject.dateLocator).first()).toBeDisabled();
            expect(pageObject.receiverLocator.element).toBeSelect2Disabled();
            expect(pageObject.roleLocator.element).toBeSelect2Disabled();
            expect(element.all(pageObject.subjectLocator).first()).toBeDisabled();
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itContractWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should save when active is clicked", () => {
            // arrange

            // act
            element.all(pageObject.activeLocator).click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/advice/1" });
        });

        it("should save when name looses focus", () => {
            // arrange

            // act
            pageObject.adviceRepeater
                .selectFirst(pageObject.nameLocator)
                .sendKeys(`SomeInput${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/advice/1" });
        });

        it("should save when date looses focus", () => {
            // arrange

            // act
            pageObject.adviceRepeater
                .selectFirst(pageObject.dateLocator)
                .sendKeys(`01-01-2016{protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/advice/1" });
        });

        it("should save when receiver is changed", () => {
            // arrange

            // act
            pageObject.receiverLocator.selectFirst();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/advice/1" });
        });

        it("should save when role is changed", () => {
            // arrange

            // act
            pageObject.roleLocator.selectFirst();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/advice/1" });
        });

        it("should save when date looses focus", () => {
            // arrange

            // act
            pageObject.adviceRepeater
                .selectFirst(pageObject.subjectLocator)
                .sendKeys(`SomeInput${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/advice/1" });
        });

        it("should delete advice when delete confirmed", () => {
            // arrange
            pageObject.adviceRepeater
                .selectFirst(pageObject.deleteButtonLocator)
                .click();

            // act
            browserHelper.acceptAlert();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/advice/1" });
        });

        it("should not delete right when delete dismissed", () => {
            // arrange
            pageObject.adviceRepeater
                .selectFirst(pageObject.deleteButtonLocator)
                .click();

            // act
            browserHelper.dismissAlert();

            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/advice/1" });
        });

        it("should create new advice when new is clicked", () => {
            // arrange

            // act
            pageObject.addButton.click();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/advice" });
        });
    });
});
