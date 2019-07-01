"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../../helper");
var PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-phases.po");
describe("project edit tab phases", function () {
    var mockHelper;
    var pageObject;
    var mockDependencies = [
        "itproject",
        "itprojectrole",
        "itprojecttype",
        "itprojectright",
        "itprojectstatus",
        "assignment"
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
        it("should hide edit phase names option", function () {
            // arrange
            // act
            // assert
            expect(pageObject.editPhaseNamesElement).not.toBeVisible();
        });
        it("should disable phase buttons", function () {
            // arrange
            var buttons = pageObject.buttonRepeater.select(1, pageObject.buttonLocator);
            // act
            // assert
            buttons.each(function (element) { return expect(element).toBeDisabled(); });
        });
        it("should disable phase cross date", function () {
            // arrange
            // select index 1 as the first element in the repeater is hidden
            var elements = pageObject.crossDateRepeater.select(1, pageObject.crossDateLocator);
            // act
            // assert
            elements.each(function (element) { return expect(element).toBeDisabled(); });
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should show input fields when edit phase names is clicked", function () {
            // arrange
            // act
            pageObject.editPhaseNamesElement.click()
                .then(function () {
                // assert
                var count = pageObject.nameRepeater.selectFirst(pageObject.nameLocator).count();
                expect(count).toBeGreaterThan(0);
            });
        });
        it("should hide input fields when edit phase names is clicked twice", function () {
            // arrange
            // act
            pageObject.editPhaseNamesElement.click();
            pageObject.editPhaseNamesElement.click();
            // assert
            var count = pageObject.nameRepeater.selectFirst(pageObject.nameLocator).count();
            expect(count).toBe(0);
        });
        it("should save when phase name looses focus", function () {
            // arrange
            pageObject.editPhaseNamesElement.click();
            pageObject.nameRepeater
                .selectFirst(pageObject.nameLocator)
                .first()
                .sendKeys("SomeText");
            // act
            pageObject.editPhaseNamesElement.click();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "POST", url: "api/itproject/1" });
        });
        it("should save when phase button is clicked", function () {
            // arrange
            // act
            pageObject.buttonRepeater.select(1, pageObject.buttonLocator).click();
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });
        it("should save when phase cross date is changed", function () {
            // arrange
            // select index 1 as the first element in the repeater is hidden
            var element = pageObject.crossDateRepeater.select(1, pageObject.crossDateLocator).first();
            // act
            element.sendKeys("1")
                .then(function () {
                // assert
                expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/itproject/1?(.*?)phaseNum=1" });
                expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/itproject/1?(.*?)phaseNum=2" });
            });
        });
    });
});
