import mock = require("protractor-http-mock");
import Helper = require("../../helper");
import ItContractEditPo = require("../../../app/components/it-contract/it-contract-edit.po");

describe("contract edit view", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: ItContractEditPo;
    var mockDependencies: Array<string> = [
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

    beforeEach(() => {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);
        pageObject = new ItContractEditPo();
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

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itContractWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should not delete when delete confirm popup is dismissed", () => {
            // arrange
            pageObject.deleteContractButton.first().click();

            // act
            browserHelper.dismissAlert();

            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itcontract/1" });
        });

        it("should delete when delete confirm popup is accepted", () => {
            // arrange
            pageObject.deleteContractButton.first().click();

            // act
            browserHelper.acceptAlert();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itcontract/1" });
        });

        it("should save when name looses focus", () => {
            // arrange

            // act
            pageObject.nameInput(`SomeInput${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when id looses focus", () => {
            // arrange

            // act
            pageObject.idInput(`SomeInput${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when parent is changed", () => {
            // arrange

            // act
            pageObject.parentSelector.selectFirst("c");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when type is changed", () => {
            // arrange

            // act
            pageObject.typeSelector.selectFirst();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when esdh looses focus", () => {
            // arrange

            // act
            pageObject.esdhInput(`SomeInput${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when folder looses focus", () => {
            // arrange

            // act
            pageObject.folderInput(`SomeInput${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when note looses focus", () => {
            // arrange

            // act
            pageObject.noteInput(`SomeInput${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when purchaseform is changed", () => {
            // arrange

            // act
            pageObject.purchaseformSelector.selectFirst();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when strategy is changed", () => {
            // arrange

            // act
            pageObject.strategySelector.selectFirst();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when plan is changed", () => {
            // arrange

            // act
            pageObject.planSelector.selectFirst();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when template is changed", () => {
            // arrange

            // act
            pageObject.templateSelector.selectFirst();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when supplier is changed", () => {
            // arrange

            // act
            pageObject.supplierSelector.selectFirst("f");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when externSigner looses focus", () => {
            // arrange

            // act
            pageObject.externSignerInput(`SomeInput${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when orgUnit is changed", () => {
            // arrange

            // act
            pageObject.orgUnitSelector.selectFirst();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when internSigner is changed", () => {
            // arrange

            // act
            pageObject.internSignerSelector.selectFirst("t");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when externSign checkbox is changed", () => {
            // arrange

            // act
            pageObject.externSignCheckbox.click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when externSignDate looses focus", () => {
            // arrange

            // act
            pageObject.externSignDateInput(`01-01-2016${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when internSign checkbox is changed", () => {
            // arrange

            // act
            pageObject.internSignCheckbox.click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when internSignDate looses focus", () => {
            // arrange

            // act
            pageObject.internSignDateInput(`01-01-2016${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should save when agreementElements is changed", () => {
            // arrange

            // act
            pageObject.agreementElementsSelector.selectFirst("l");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "POST", url: "api/itcontract/1(.)+elemId=[0-9]+" });
        });
    });
});
