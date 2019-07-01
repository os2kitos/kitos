"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../../helper");
var PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-handover.po");
describe("project edit tab handover", function () {
    var mockHelper;
    var pageObject;
    var mockDependencies = [
        "itproject",
        "itprojecttype",
        "itprojectrole",
        "itprojectright",
        "handover"
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
        it("should disable inputs", function () {
            // arrange
            // act
            // assert
            expect(pageObject.descriptionElement).toBeDisabled();
            expect(pageObject.meetingElement).toBeDisabled();
            expect(pageObject.participantsSelect.element).toBeSelect2Disabled();
            expect(pageObject.summaryElement).toBeDisabled();
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should save when description looses focus", function () {
            // arrange
            pageObject.descriptionInput("SomeDescription");
            // act
            pageObject.descriptionElement.sendKeys(protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/handover/1" });
        });
        it("should save when meeting data looses focus", function () {
            // arrange
            pageObject.meetingInput("01-01-2017");
            // act
            pageObject.meetingElement.sendKeys(protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/handover/1" });
        });
        it("should save when participants changes", function () {
            // arrange
            // act
            pageObject.participantsSelect.selectFirst();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "POST", url: "api/handover/1" });
        });
        it("should save when summary changes", function () {
            // arrange
            pageObject.summaryInput("SomeDescription");
            // act
            pageObject.summaryElement.sendKeys(protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/handover/1" });
        });
        it("should add participant to tags when clicked", function () {
            // arrange
            // act
            pageObject.participantsSelect.selectFirst();
            // assert
            expect(pageObject.participantsSelect.selectedOptions().count()).not.toBe(0);
        });
    });
});
