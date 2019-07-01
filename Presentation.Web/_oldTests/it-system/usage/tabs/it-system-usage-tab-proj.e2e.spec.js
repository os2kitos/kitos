"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../../helper");
var ItSystemUsageTabProjPo = require("../../../../app/components/it-system/usage/tabs/it-system-usage-tab-proj.po");
describe("system usage tab proj", function () {
    var mockHelper;
    var browserHelper;
    var pageObject;
    var mockDependencies = [
        "itSystemUsage",
        "businesstype",
        "archivetype",
        "sensitivedatatype",
        "itproject"
    ];
    beforeEach(function () {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);
        pageObject = new ItSystemUsageTabProjPo();
        browser.driver.manage().window().maximize();
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
        it("should hide project delete button", function () {
            // arrange
            var deleteButton = pageObject.projectRepeater.selectFirst(pageObject.deleteLocator).first();
            // act
            // assert
            expect(deleteButton).not.toBeVisible();
        });
        it("should hide project selector", function () {
            // arrange
            // act
            // assert
            expect(pageObject.projectSelector.element.element(by.xpath("../../.."))).toHaveClass("ng-hide");
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itSystemUsageWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should save when project is selected", function () {
            // arrange
            // act
            pageObject.projectSelector.selectFirst("p");
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/itproject/[0-9]+" });
        });
        it("should repeat selected projects", function () {
            // arrange
            // act
            // assert
            expect(pageObject.projectRepeater.count()).toBeGreaterThan(0, "Selected projects are not repeated");
        });
        it("should not delete when delete confirm popup is dismissed", function () {
            // arrange
            pageObject.projectRepeater.selectFirst(pageObject.deleteLocator).first().click();
            // act
            browserHelper.dismissAlert();
            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itproject/[0-9]+" });
        });
        it("should delete when delete confirm popup is accepted", function () {
            // arrange
            pageObject.projectRepeater.selectFirst(pageObject.deleteLocator).first().click();
            // act
            browserHelper.acceptAlert();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itproject/[0-9]+" });
        });
    });
});
