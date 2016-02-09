import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import ItSystemUsageTabInterfacesPo = require("../../../../app/components/it-system/usage/tabs/it-system-usage-tab-interfaces.po");

describe("system usage tab interfaces", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: ItSystemUsageTabInterfacesPo;
    var mockDependencies: Array<string> = [
        "itSystemUsage",
        "method",
        "interface",
        "frequency",
        "businesstype",
        "interfacetype",
        "archivetype",
        "datatype",
        "sensitivedatatype",
        "tsa",
        "itInterfaceUse",
        "exhibit",
        "interfaceUsage"
    ];

    beforeEach(() => {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);

        pageObject = new ItSystemUsageTabInterfacesPo();

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
            pageObject.usageRepeater.each(row => {
                expect(row.element(pageObject.wishForLocator)).toBeDisabled();
            });
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itSystemUsageWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should save when checkbox is checked", () => {
            // arrange

            // act
            pageObject.usageRepeater
                .selectFirst(pageObject.wishForLocator)
                .first()
                .click();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "PATCH", url: "api/interfaceUsage/" });
        });
    });
});
