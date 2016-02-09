import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import ItSystemEditTabInterfacesPo = require("../../../../app/components/it-system/edit/tabs/it-system-edit-tab-interfaces.po");

describe("system edit view tab interfaces", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: ItSystemEditTabInterfacesPo;
    var mockDependencies: Array<string> = [
        "itSystem",
        "businessType",
        "itSystemTypeOption",
        "itInterfaceUse",
        "organization",
        "itInterface"
    ];

    beforeEach(() => {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);

        pageObject = new ItSystemEditTabInterfacesPo();

        browser.driver.manage().window().maximize();
    });

    afterEach(() => {
        mock.teardown();
    });

    describe("with no write access", () => {
        beforeEach(done => {
            mock(["itSystemNoWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should hide inputs", () => {
            // arrange

            // act

            // assert
            expect(pageObject.selectInterface.element.element(by.xpath("../../.."))).toHaveClass("ng-hide");
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itSystemWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should save when interface is selected", () => {
            // arrange

            // act
            pageObject.selectInterface.selectFirst("i");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "POST", url: "api/itInterfaceUse/" });
        });

        it("should not delete when delete confirm popup is dismissed", () => {
            // arrange
            pageObject.interfaceRepeater.selectFirst(pageObject.deleteInterfaceLocator).first().click();

            // act
            browserHelper.dismissAlert();

            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itInterfaceUse/" });
        });

        it("should delete when delete confirm popup is accepted", () => {
            // arrange
            pageObject.interfaceRepeater.selectFirst(pageObject.deleteInterfaceLocator).first().click();

            // act
            browserHelper.acceptAlert();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itInterfaceUse/" });
        });
    });
});
