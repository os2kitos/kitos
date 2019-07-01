"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../../helper");
var PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-itsys.po");
describe("project edit tab it system", function () {
    var mockHelper;
    var browserHelper;
    var pageObject;
    var mockDependencies = [
        "itproject",
        "itprojecttype",
        "itSystemUsage"
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
            mock(["itProjectNoWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should hide selector", function () {
            // arrange
            // act
            // assert
            expect(pageObject.usageSelect.element).not.toBeVisible();
        });
        it("should hide delete button", function () {
            // arrange
            // act
            // assert
            var button = pageObject.usageRepeater
                .selectFirst(pageObject.deleteLocator)
                .first();
            expect(button).not.toBeVisible();
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should repeat project usages", function () {
            // arrange
            // act
            // assert
            expect(pageObject.usageRepeater.count()).toBeGreaterThan(0);
        });
        it("should delete usage on confirmed delete click", function () {
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
        it("should not delete usage on dismissed delete click", function () {
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
        it("should search for it systems when entering in selector", function () {
            // arrange
            var query = "i";
            // act
            pageObject.usageSelect.writeQuery(query);
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "GET", url: "api/itSystemUsage?(.)*q=" + query });
        });
        it("should save usage on selector click", function () {
            // arrange
            // act
            pageObject.usageSelect.selectFirst("i");
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/itproject?(.)*usageId=[0-9]" });
        });
    });
});
