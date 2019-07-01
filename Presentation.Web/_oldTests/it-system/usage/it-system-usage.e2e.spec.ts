import mock = require("protractor-http-mock");
import Helper = require("../../helper");
import ItSystemUsagePo = require("../../../app/components/it-system/usage/it-system-usage.po");

describe("system usage view", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: ItSystemUsagePo;
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

        pageObject = new ItSystemUsagePo();

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

        it("should disable inputs", () => {
            // arrange

            // act

            // assert
            expect(pageObject.localSystemIdElement).toBeDisabled();
            expect(pageObject.localCallNameElement).toBeDisabled();
            expect(pageObject.sensitiveSelector.element).toBeSelect2Disabled();
            expect(pageObject.esdhElement).toBeDisabled();
            expect(pageObject.linkElement).toBeDisabled();
            expect(pageObject.versionElement).toBeDisabled();
            expect(pageObject.usageOwnerElement).toBeDisabled();
            expect(pageObject.overviewSelector.element).toBeSelect2Disabled();
            expect(pageObject.archiveSelector.element).toBeSelect2Disabled();
            expect(pageObject.cmdbElement).toBeDisabled();
            expect(pageObject.noteElement).toBeDisabled();
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itSystemUsageWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should not delete when delete confirm popup is dismissed", () => {
            // arrange
            pageObject.deleteUsageElement.first().click();

            // act
            browserHelper.dismissAlert();

            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itSystemUsage/1" });
        });

        it("should delete when delete confirm popup is accepted", () => {
            // arrange
            pageObject.deleteUsageElement.first().click();

            // act
            browserHelper.acceptAlert();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itSystemUsage/1" });
        });

        it("should save when local system id looses focus", () => {
            // arrange

            // act
            pageObject.localSystemIdInput(`2${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itSystemUsage/1" });
        });

        it("should save when local name looses focus", () => {
            // arrange

            // act
            pageObject.localCallNameInput(`SomeName${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itSystemUsage/1" });
        });

        it("should save when sensitive data selector is changed", () => {
            // arrange

            // act
            pageObject.sensitiveSelector.selectFirst("n");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itSystemUsage/1" });
        });

        it("should save when esdh looses focus", () => {
            // arrange

            // act
            pageObject.esdhInput(`SomeEsdh${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itSystemUsage/1" });
        });

        it("should save when link looses focus", () => {
            // arrange

            // act
            pageObject.linkInput(`SomeLink${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itSystemUsage/1" });
        });

        it("should save when version looses focus", () => {
            // arrange

            // act
            pageObject.versionInput(`2.0${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itSystemUsage/1" });
        });

        it("should save when overview selector is changed", () => {
            // arrange

            // act
            pageObject.overviewSelector.selectFirst("i");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itSystemUsage/1" });
        });

        it("should save when archive selector changes", () => {
            // arrange

            // act
            pageObject.archiveSelector.selectFirst("g");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itSystemUsage/1" });
        });

        it("should save when cmdb looses focus", () => {
            // arrange

            // act
            pageObject.cmdbInput(`SomeCmdb${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itSystemUsage/1" });
        });

        it("should save when note looses focus", () => {
            // arrange

            // act
            pageObject.noteInput(`SomeNote${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itSystemUsage/1" });
        });
    });
});
