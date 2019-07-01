"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../../helper");
var PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-org.po");
describe("project edit tab org", function () {
    var mockHelper;
    var pageObject;
    var mockDependencies = [
        "itproject",
        "itprojecttype",
        "itProjectOrgUnitUsage",
        "organizationunit"
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
            mock(["itProjectNoWriteAccess"].concat(mockDependencies));
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
            elements.each(function (element, index) {
                expect(element).toBeDisabled();
            });
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should save when organization is selected", function () {
            // arrange
            // act
            pageObject.orgUnitsTreeRepeater.selectFirst(pageObject.orgUnitLocator).click();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "POST", url: "api/itproject/1" });
        });
        it("should save when organization is deselected", function () {
            // arrange
            pageObject.orgUnitsTreeRepeater.selectFirst(pageObject.orgUnitLocator).click();
            // act
            pageObject.orgUnitsTreeRepeater.selectFirst(pageObject.orgUnitLocator).click();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "DELETE", url: "api/itproject/1" });
        });
        it("should present no units in dropdown when organization is deselected", function () {
            // arrange
            pageObject.orgUnitsTreeRepeater.selectFirst(pageObject.orgUnitLocator).click();
            pageObject.orgUnitsTreeRepeater.selectFirst(pageObject.orgUnitLocator).click();
            // act
            pageObject.responsibleOrgSelector.click();
            var selectorCount = pageObject.responsibleOrgSelector.options.count();
            // assert
            expect(selectorCount).toBe(0, "selector has options");
        });
        it("should present units in dropdown when organization is selected", function () {
            // arrange
            pageObject.orgUnitsTreeRepeater.selectFirst(pageObject.orgUnitLocator).click();
            // act
            pageObject.responsibleOrgSelector.click();
            var selectorCount = pageObject.responsibleOrgSelector.options.count();
            // assert
            expect(selectorCount).toBeGreaterThan(0, "selector hos no options");
        });
    });
});
