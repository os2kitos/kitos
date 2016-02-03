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
        "interfaceUsage"
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
            browser.sleep(3000);
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
            mock(["itContractNoWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        //it("should not delete when delete confirm popup is dismissed", () => {
        //    // arrange
        //    pageObject.deleteContractButton.first().click();

        //    // act
        //    browserHelper.dismissAlert();

        //    // assert
        //    expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itcontract/1" });
        //});

        //it("should delete when delete confirm popup is accepted", () => {
        //    // arrange
        //    pageObject.deleteContractButton.first().click();

        //    // act
        //    browserHelper.acceptAlert();

        //    // assert
        //    expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itcontract/1" });
        //});

        //it("should save when name looses focus", () => {
        //    // arrange
        //    pageObject.nameInput("SomeName");

        //    // act
        //    pageObject.idElement.click();

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        //});

        //it("should save when projectId looses focus", () => {
        //    // arrange
        //    pageObject.idInput("SomeId");

        //    // act
        //    pageObject.nameElement.click();

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        //});

        //it("should save when type is changed", () => {
        //    // arrange

        //    // act
        //    pageObject.typeSelect.selectFirst("lo");

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        //});

        //it("should save when cmdb looses focus", () => {
        //    // arrange
        //    pageObject.cmdbInput("SomeCmdb");

        //    // act
        //    pageObject.nameElement.click();

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        //});

        //it("should save when access is changed", () => {
        //    // arrange

        //    // act
        //    pageObject.accessSelect.selectFirst("p");

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        //});

        //it("should save when esdh looses focus", () => {
        //    // arrange
        //    pageObject.esdhInput("SomeEsdh");

        //    // act
        //    pageObject.nameElement.click();

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        //});

        //it("should save when folder looses focus", () => {
        //    // arrange
        //    pageObject.folderInput("SomeFolder");

        //    // act
        //    pageObject.nameElement.click();

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        //});

        //it("should save when background looses focus", () => {
        //    // arrange
        //    pageObject.backgroundInput("SomeBackground");

        //    // act
        //    pageObject.nameElement.click();

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        //});

        //it("should save when note looses focus", () => {
        //    // arrange
        //    pageObject.noteInput("SomeNote");

        //    // act
        //    pageObject.nameElement.click();

        //    // assert
        //    expect(mockHelper.lastRequest()).toBeTruthy({ method: "PATCH", url: "api/itcontract/1" });
        //});

        //it("should save when archive checkbox is selected", () => {
        //    // arrange

        //    // act
        //    pageObject.archiveCheckbox.click();

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        //});

        //it("should save when transversal checkbox is selected", () => {
        //    // arrange

        //    // act
        //    pageObject.transversalCheckbox.click();

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        //});

        //it("should save when strategy checkbox is selected", () => {
        //    // arrange

        //    // act
        //    pageObject.strategyCheckbox.click();

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        //});

        //it("should search for projects when change in field", () => {
        //    // arrange

        //    // act
        //    pageObject.projectParentSelect.selectFirst("p");

        //    // assert
        //    expect(mock.requestsMade()).toMatchInRequests({ method: "GET", url: "api/itcontract?(.*?)q=p" });
        //});
    });
});
