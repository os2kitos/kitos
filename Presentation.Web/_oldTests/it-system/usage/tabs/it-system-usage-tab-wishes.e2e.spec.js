"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../../helper");
var PageObject = require("../../../../app/components/it-system/usage/tabs/it-system-usage-tab-wishes.po");
describe("system usage tab wishes", function () {
    var mockHelper;
    var browserHelper;
    var pageObject;
    var mockDependencies = [
        "itSystemUsage",
        "businesstype",
        "archivetype",
        "sensitivedatatype",
        "wish"
    ];
    beforeEach(function () {
        browser.driver.manage().window().maximize();
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);
        pageObject = new PageObject();
    });
    afterEach(function () {
        mock.teardown();
    });
    describe("with no write access", function () {
        beforeEach(function (done) {
            mock(["itSystemUsageNoWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should hide add wish fields", function () {
            // arrange
            // act
            // assert
            expect(pageObject.textElement).not.toBeVisible();
        });
        it("should hide delete button on wish", function () {
            // arrange
            var deleteButton = element.all(pageObject.deleteWishLocator)
                .first();
            // act
            // assert
            expect(deleteButton).not.toBeVisible();
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itSystemUsageWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should repeat wishes", function () {
            // arrange
            // act
            // assert
            expect(pageObject.wishRepeater.count()).toBeGreaterThan(0, "No repeated wishes");
        });
        it("should save on save click", function () {
            // arrange
            pageObject.textInput("NewWish");
            // act
            pageObject.saveWishButton.click();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/wish" });
        });
        it("should delete when delete confirmed", function () {
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
        it("should not delete when delete dismissed", function () {
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
