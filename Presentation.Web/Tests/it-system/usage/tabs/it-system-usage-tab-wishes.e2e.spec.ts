import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import PageObject = require("../../../../app/components/it-system/usage/tabs/it-system-usage-tab-wishes.po");

describe("system usage tab wishes", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: PageObject;
    var mockDependencies: Array<string> = [
        "itSystemUsage",
        "businesstype",
        "archivetype",
        "sensitivedatatype",
        "wish"
    ];

    beforeEach(() => {
        browser.driver.manage().window().maximize();

        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);
        pageObject = new PageObject();
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

        it("should hide add wish fields", () => {
            // arrange

            // act

            // assert
            expect(pageObject.textElement).not.toBeVisible();
        });

        it("should hide delete button on wish", () => {
            // arrange
            var deleteButton = element.all(pageObject.deleteWishLocator)
                .first();

            // act

            // assert
            expect(deleteButton).not.toBeVisible();
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itSystemUsageWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should repeat wishes", () => {
            // arrange

            // act

            // assert
            expect(pageObject.wishRepeater.count()).toBeGreaterThan(0, "No repeated wishes");
        });

        it("should save on save click", () => {
            // arrange
            pageObject.textInput("NewWish");

            // act
            pageObject.saveWishButton.click();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/wish" });
        });

        it("should delete when delete confirmed", () => {
            // arrange
            mock.clearRequests();
            pageObject.wishRepeater
                .selectFirst(pageObject.deleteWishLocator)
                .click();

            // act
            browserHelper.acceptAlert();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/wish/[0-9]+" });
        });

        it("should not delete when delete dismissed", () => {
            // arrange
            pageObject.wishRepeater
                .selectFirst(pageObject.deleteWishLocator)
                .click();

            // act
            browserHelper.dismissAlert();

            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/wish/[0-9]+" });
        });
    });
});
