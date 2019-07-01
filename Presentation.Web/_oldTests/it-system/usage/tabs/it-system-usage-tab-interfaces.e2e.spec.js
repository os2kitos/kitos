"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../../helper");
var ItSystemUsageTabInterfacesPo = require("../../../../app/components/it-system/usage/tabs/it-system-usage-tab-interfaces.po");
describe("system usage tab interfaces", function () {
    var mockHelper;
    var browserHelper;
    var pageObject;
    var mockDependencies = [
        "itSystemUsage",
        "method",
        "interface",
        "frequency",
        "businesstype",
        "interfacetype",
        "archivetype",
        "datatype",
        "sensitivedatatype",
        "tsa",
        "itInterfaceUse",
        "exhibit",
        "interfaceUsage"
    ];
    beforeEach(function () {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);
        pageObject = new ItSystemUsageTabInterfacesPo();
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
            pageObject.usageRepeater.each(function (row) {
                expect(row.element(pageObject.wishForLocator)).toBeDisabled();
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
        it("should save when checkbox is checked", function () {
            // arrange
            // act
            pageObject.usageRepeater
                .selectFirst(pageObject.wishForLocator)
                .first()
                .click();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "PATCH", url: "api/interfaceUsage/" });
        });
    });
});
