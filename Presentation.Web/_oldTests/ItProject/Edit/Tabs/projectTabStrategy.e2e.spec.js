"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../../helper");
var PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-strategy.po");
describe("project edit tab strategy", function () {
    var mockHelper;
    var pageObject;
    var mockDependencies = ["itproject", "itprojecttype", "itprojectstatus"];
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
        it("should disable inputs", function () {
            // arrange
            // act
            // assert
            expect(pageObject.jointMunicipalSelector.element).toBeSelect2Disabled();
            expect(pageObject.commonPublicSelector.element).toBeSelect2Disabled();
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should save when joint municipal strategy is clicked", function () {
            // arrange
            // act
            pageObject.jointMunicipalSelector.selectFirst();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });
        it("should save when common public strategy is clicked", function () {
            // arrange
            // act
            pageObject.commonPublicSelector.selectFirst();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });
    });
});
