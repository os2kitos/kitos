import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import ItSystemUsageTabProjPo = require("../../../../app/components/it-system/usage/tabs/it-system-usage-tab-proj.po");

describe("system usage tab proj", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: ItSystemUsageTabProjPo;
    var mockDependencies: Array<string> = [
        "itSystemUsage",
        "businesstype",
        "archivetype",
        "sensitivedatatype",
        "itproject"
    ];

    beforeEach(() => {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);

        pageObject = new ItSystemUsageTabProjPo();

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

        it("should hide project delete button", () => {
            // arrange
            var deleteButton = pageObject.projectRepeater.selectFirst(pageObject.deleteLocator).first();

            // act

            // assert
            expect(deleteButton).not.toBeVisible();
        });

        it("should hide project selector", () => {
            // arrange

            // act

            // assert
            expect(pageObject.projectSelector.element.element(by.xpath("../../.."))).toHaveClass("ng-hide");
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itSystemUsageWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should save when project is selected", () => {
            // arrange

            // act
            pageObject.projectSelector.selectFirst("p");

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/itproject/[0-9]+" });
        });

        it("should repeat selected projects", () => {
            // arrange

            // act

            // assert
            expect(pageObject.projectRepeater.count()).toBeGreaterThan(0, "Selected projects are not repeated");
        });

        it("should not delete when delete confirm popup is dismissed", () => {
            // arrange
            pageObject.projectRepeater.selectFirst(pageObject.deleteLocator).first().click();

            // act
            browserHelper.dismissAlert();

            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itproject/[0-9]+" });
        });

        it("should delete when delete confirm popup is accepted", () => {
            // arrange
            pageObject.projectRepeater.selectFirst(pageObject.deleteLocator).first().click();

            // act
            browserHelper.acceptAlert();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itproject/[0-9]+" });
        });
    });
});
