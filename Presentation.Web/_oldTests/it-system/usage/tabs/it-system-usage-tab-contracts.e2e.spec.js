"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../../helper");
var ItSystemUsageTabContractsPo = require("../../../../app/components/it-system/usage/tabs/it-system-usage-tab-contracts.po");
describe("system usage tab contracts", function () {
    var mockHelper;
    var browserHelper;
    var pageObject;
    var mockDependencies = [
        "itSystemUsage",
        "businesstype",
        "archivetype",
        "sensitivedatatype",
        "itInterfaceUse",
        "itContractItSystemUsage"
    ];
    beforeEach(function () {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);
        pageObject = new ItSystemUsageTabContractsPo();
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
        it("should disable checkboxes", function () {
            // arrange
            // act
            // assert
            expect(pageObject.contractSelector.element).toBeSelect2Disabled();
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itSystemUsageWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should save when contract is selected", function () {
            // arrange
            // act
            pageObject.contractSelector.selectFirst();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/ItContractItSystemUsage/" });
        });
        it("should repeat selected contracts", function () {
            // arrange
            // act
            // assert
            expect(pageObject.contractsRepeater.count()).toBeGreaterThan(0, "Selected contracts are not repeated");
        });
    });
});
