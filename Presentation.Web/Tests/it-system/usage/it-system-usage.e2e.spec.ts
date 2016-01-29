import mock = require("protractor-http-mock");
import Helper = require("../../helper");
import ItSystemUsagePo = require("../../../app/components/it-system/usage/it-system-usage.po");

describe("system edit view", () => {
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
        beforeEach(() => {
            mock(["itSystemUsageNoWriteAccess"].concat(mockDependencies));
            pageObject.getPage();

            // clear initial requests
            //mock.clearRequests();
        });

        it("should disable inputs", () => {
            // arrange
            browserHelper.outputLog();
            mockHelper.outputRequests();
            // act
            browser.sleep(3000);
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
        beforeEach(() => {
            mock(["itSystemUsageWriteAccess"].concat(mockDependencies));
            pageObject.getPage();

            // clear initial requests
            browser.sleep(300);
            mock.clearRequests();
        });

        //it("should not delete when delete confirm popup is dismissed", () => {
        //    // arrange
        //    pageObject.deleteSystemElement.first().click();

        //    // act
        //    browserHelper.dismissAlert();

        //    // assert
        //    expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itsystem/1" });
        //});

        //it("should delete when delete confirm popup is accepted", () => {
        //    // arrange
        //    pageObject.deleteSystemElement.first().click();

        //    // act
        //    browserHelper.acceptAlert();

        //    // assert
        //    expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itsystem/1" });
        //});

        //it("should save when appType changes", () => {
        //    // arrange

        //    // act
        //    pageObject.appTypeSelect.selectFirst("i");

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        //});

        //it("should save when name looses focus", () => {
        //    // arrange

        //    // act
        //    pageObject.nameInput("SomeName");

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        //});

        //it("should save when system parent is changed", () => {
        //    // arrange

        //    // act
        //    pageObject.systemParentSelect.selectFirst("i");

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        //});

        //it("should save when belongs to is changed", () => {
        //    // arrange

        //    // act
        //    pageObject.belongsToSelect.selectFirst("i");

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        //});

        //it("should save when access modifier is changed", () => {
        //    // arrange

        //    // act
        //    pageObject.accessModifierSelect.selectFirst("p");

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        //});

        //it("should save when usage type is changed", () => {
        //    // arrange

        //    // act
        //    pageObject.usageTypeSelector.selectFirst("p");

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        //});

        //it("should save when description looses focus", () => {
        //    // arrange

        //    // act
        //    pageObject.descriptionInput("SomeDescription");

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        //});

        //it("should save when further description looses focus", () => {
        //    // arrange

        //    // act
        //    pageObject.furtherDescriptionInput("SomeFurtherDescription");

        //    // assert
        //    expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        //});
    });
});
