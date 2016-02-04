import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import ItContractEditTabSystemPo = require("../../../../app/components/it-contract/tabs/it-contract-tab-systems.po");

describe("contract edit tab systems", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: ItContractEditTabSystemPo;
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
        "itSystemUsage",
        "exhibit",
        "itInterfaceExhibitUsage"
    ];

    beforeEach(() => {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);

        pageObject = new ItContractEditTabSystemPo();

        browser.driver.manage().window().maximize();
    });

    afterEach(() => {
        mock.teardown();
    });

    //describe("with no write access", () => {
    //    beforeEach(done => {
    //        mock(["itContractNoWriteAccess"].concat(mockDependencies));
    //        pageObject.getPage()
    //            .then(() => mock.clearRequests())
    //            .then(() => done());
    //    });

    //    it("should hide inputs", () => {
    //        // arrange

    //        // act

    //        // assert
    //        expect(pageObject.systemUsageSelector.element).not.toBeVisible();
    //        expect(pageObject.newInterfaceInterfaceUsageSelector.element).not.toBeVisible();
    //        expect(pageObject.newInterfaceUsageTypeSelector.element).not.toBeVisible();
    //        expect(pageObject.newInterfaceInterfaceUsageSelector.element).not.toBeVisible();
    //    });

    //    it("should hide delete buttons", () => {
    //        // arrange

    //        // act

    //        // assert
    //        pageObject.systemUsageRepeater.each(row => {
    //            expect(row.element(pageObject.deleteUsageLocator)).not.toBeVisible();
    //        });

    //        pageObject.interfaceExhibitRepeater.each(row => {
    //            expect(row.element(pageObject.deleteInterfaceExhibitLocator)).not.toBeVisible();
    //        });

    //        pageObject.interfaceRepeater.each(row => {
    //            expect(row.element(pageObject.deleteInterfaceUsageLocator)).not.toBeVisible();
    //        });
    //    });
    //});

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itContractWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        //it("should save when systemUsage is changed", () => {
        //    // arrange

        //    // act
        //    pageObject.systemUsageSelector.selectFirst("i");

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "POST", url: "api/itcontract/1" });
        //});

        it("should save when interface relation is completed", () => {
            // arrange

            // act
            pageObject.newInterfaceSystemUsageSelector.selectFirst("i");
            pageObject.newInterfaceUsageTypeSelector.selectFirst();
            pageObject.newInterfaceInterfaceUsageSelector.selectFirst("i");

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "PATCH", url: "api/itInterfaceExhibitUsage" });
        });
    });
});
