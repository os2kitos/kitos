import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-itsys.po");

describe("project edit tab it system", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: PageObject;
    var mockDependencies: Array<string> = [
        "itproject",
        "itprojecttype",
        "itSystemUsage"
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
            mock(["itProjectNoWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should hide selector", () => {
            // arrange

            // act

            // assert
            expect(pageObject.usageSelect.element).not.toBeVisible();
        });

        it("should hide delete button", () => {
            // arrange

            // act

            // assert
            var button = pageObject.usageRepeater
                .selectFirst(pageObject.deleteLocator)
                .first();
            expect(button).not.toBeVisible();
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should repeat project usages", () => {
            // arrange

            // act

            // assert
            expect(pageObject.usageRepeater.count()).toBeGreaterThan(0);
        });

        it("should delete usage on confirmed delete click", () => {
            // arrange
            pageObject.usageRepeater
                .selectFirst(pageObject.deleteLocator)
                .first()
                .click();

            // act
            browserHelper.acceptAlert();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itproject/1?(.)*usageId=[0-9]" });
        });

        it("should not delete usage on dismissed delete click", () => {
            // arrange
            pageObject.usageRepeater
                .selectFirst(pageObject.deleteLocator)
                .first()
                .click();

            // act
            browserHelper.dismissAlert();

            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itproject/1?(.)*usageId=[0-9]" });
        });

        it("should search for it systems when entering in selector", () => {
            // arrange
            var query = "i";

            // act
            pageObject.usageSelect.writeQuery(query);

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "GET", url: "api/itSystemUsage?(.)*q=" + query });
        });

        it("should save usage on selector click", () => {
            // arrange

            // act
            pageObject.usageSelect.selectFirst("i");

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/itproject?(.)*usageId=[0-9]" });
        });
    });
});
