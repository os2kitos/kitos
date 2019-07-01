"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../../helper");
var PageObject = require("../../../../app/components/it-system/usage/tabs/it-system-usage-tab-org.po");
describe("system usage tab org", function () {
    var mockHelper;
    var pageObject;
    var mockDependencies = [
        "itSystemUsage",
        "businesstype",
        "archivetype",
        "sensitivedatatype",
        "organizationunit",
        "itSystemUsageOrgUnitUsage"
    ];
    beforeEach(function () {
        browser.driver.manage().window().maximize();
        mockHelper = new Helper.Mock();
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
        it("should disable responsible organization dropdown", function () {
            // arrange
            // act
            // assert
            expect(pageObject.responsibleOrgSelector.element).toBeSelect2Disabled();
        });
        it("should disable organization elements", function () {
            // arrange
            var elements = element.all(pageObject.orgUnitLocator);
            // act
            // assert
            elements.each(function (element) {
                expect(element).toBeDisabled();
            });
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itSystemUsageWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should save when organization is selected", function () {
            // arrange
            // act
            pageObject.orgUnitsTreeRepeater.selectFirst(pageObject.orgUnitLocator).first().click();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "POST", url: "api/itSystemUsage/1" });
        });
        it("should save when organization is deselected", function () {
            // arrange
            pageObject.orgUnitsTreeRepeater.selectFirst(pageObject.orgUnitLocator).first().click();
            // act
            pageObject.orgUnitsTreeRepeater.selectFirst(pageObject.orgUnitLocator).first().click();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "DELETE", url: "api/itSystemUsage/1" });
        });
        it("should present no units in dropdown when organization is deselected", function () {
            // arrange
            pageObject.orgUnitsTreeRepeater.selectFirst(pageObject.orgUnitLocator).first().click();
            pageObject.orgUnitsTreeRepeater.selectFirst(pageObject.orgUnitLocator).first().click();
            // act
            pageObject.responsibleOrgSelector.click();
            var selectorCount = pageObject.responsibleOrgSelector.options.count();
            // assert
            expect(selectorCount).toBe(0, "selector has options");
        });
        it("should present units in dropdown when organization is selected", function () {
            // arrange
            pageObject.orgUnitsTreeRepeater.selectFirst(pageObject.orgUnitLocator).first().click();
            // act
            pageObject.responsibleOrgSelector.click();
            var selectorCount = pageObject.responsibleOrgSelector.options.count();
            // assert
            expect(selectorCount).toBeGreaterThan(0, "selector hos no options");
        });
    });
});
