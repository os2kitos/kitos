import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import ItSystemUsageTabContractsPo = require("../../../../app/components/it-system/usage/tabs/it-system-usage-tab-contracts.po");

describe("system usage tab contracts", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: ItSystemUsageTabContractsPo;
    var mockDependencies: Array<string> = [
        "itSystemUsage",
        "businesstype",
        "archivetype",
        "sensitivedatatype",
        "itInterfaceUse",
        "itContractItSystemUsage"
    ];

    beforeEach(() => {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);

        pageObject = new ItSystemUsageTabContractsPo();

        browser.driver.manage().window().maximize();
    });

    afterEach(() => {
        mock.teardown();
    });

    describe("with no write access", () => {
        beforeEach(done => {
            mock(["itSystemUsageNoWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should disable checkboxes", () => {
            // arrange

            // act

            // assert
            expect(pageObject.contractSelector.element).toBeSelect2Disabled();
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itSystemUsageWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should save when contract is selected", () => {
            // arrange

            // act
            pageObject.contractSelector.selectFirst();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/ItContractItSystemUsage/" });
        });

        it("should repeat selected contracts", () => {
            // arrange

            // act

            // assert
            expect(pageObject.contractsRepeater.count()).toBeGreaterThan(0, "Selected contracts are not repeated");
        });
    });
});
