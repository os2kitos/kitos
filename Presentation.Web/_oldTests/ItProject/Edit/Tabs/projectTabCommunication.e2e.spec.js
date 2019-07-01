"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../../helper");
var PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-communication.po");
describe("project edit tab communication", function () {
    var mockHelper;
    var browserHelper;
    var pageObject;
    var mockDependencies = [
        "itproject",
        "itprojecttype",
        "itprojectright",
        "itprojectrole",
        "itprojectstatus",
        "communication"
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
        it("should hide input fields", function () {
            // arrange
            // act
            // assert
            expect(pageObject.targetElement).not.toBeVisible();
            expect(pageObject.purposeElement).not.toBeVisible();
            expect(pageObject.messageElement).not.toBeVisible();
            expect(pageObject.mediaElement).not.toBeVisible();
            expect(pageObject.dueDateElement).not.toBeVisible();
            expect(pageObject.responsibleSelect.element).not.toBeVisible();
        });
        it("should disable repeated comm input fields", function () {
            // arrange
            // act
            // assert
            expect(pageObject.commRepeater.selectFirst(pageObject.targetLocator).first()).toBeDisabled();
            expect(pageObject.commRepeater.selectFirst(pageObject.purposeLocator).first()).toBeDisabled();
            expect(pageObject.commRepeater.selectFirst(pageObject.messageLocator).first()).toBeDisabled();
            expect(pageObject.commRepeater.selectFirst(pageObject.mediaLocator).first()).toBeDisabled();
            expect(pageObject.commRepeater.selectFirst(pageObject.dueDateLocator).first()).toBeDisabled();
            pageObject.getResponsibleSelect(0).then(function (s) { return expect(s.element).toBeSelect2Disabled(); });
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should mark required inputs on save when nothing is entered", function () {
            // arrange
            // act
            pageObject.saveCommElement.click()
                .then(function () {
                // assert
                expect(pageObject.dueDateElement.element(by.xpath("../../.."))).toHaveClass("has-error");
                expect(pageObject.responsibleSelect.element.element(by.xpath(".."))).toHaveClass("has-error");
            });
        });
        it("should save when save is clicked", function () {
            // arrange
            // below is dummy. Hardcoded values are returned from mock response
            pageObject.dueDateInput("01-01-2016");
            pageObject.responsibleSelect.selectFirst();
            // act
            pageObject.saveCommElement.click()
                .then(function () {
                // assert
                expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/communication" });
            });
        });
        it("should repeat comms", function () {
            // arrange
            // act
            // assert
            expect(pageObject.commRepeater.count()).toBeGreaterThan(0);
        });
        it("should delete comm when delete is confirmed", function () {
            // arrange
            pageObject.commRepeater
                .selectFirst(pageObject.deleteCommLocator)
                .first()
                .click();
            // act
            browserHelper.acceptAlert();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/communication/" });
        });
        it("should not delete comm when delete is dismissed", function () {
            // arrange
            pageObject.commRepeater
                .selectFirst(pageObject.deleteCommLocator)
                .first()
                .click();
            // act
            browserHelper.dismissAlert();
            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/communication/" });
        });
        it("should save when target looses focus", function () {
            // arrange
            // act
            pageObject.commRepeater
                .selectFirst(pageObject.targetLocator)
                .sendKeys("NewTarget", protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/communication/" });
        });
        it("should save when purpose looses focus", function () {
            // arrange
            // act
            pageObject.commRepeater
                .selectFirst(pageObject.purposeLocator)
                .sendKeys("NewPurpose", protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/communication/" });
        });
        it("should save when message looses focus", function () {
            // arrange
            // act
            pageObject.commRepeater
                .selectFirst(pageObject.messageLocator)
                .sendKeys("NewMessage", protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/communication/" });
        });
        it("should save when media looses focus", function () {
            // arrange
            // act
            pageObject.commRepeater
                .selectFirst(pageObject.mediaLocator)
                .sendKeys("NewMedia", protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/communication/" });
        });
        it("should save when due date looses focus", function () {
            // arrange
            // act
            pageObject.commRepeater
                .selectFirst(pageObject.dueDateLocator)
                .sendKeys(protractor.Key.CONTROL, "a", protractor.Key.NULL, "01-01-2017", protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/communication/" });
        });
        it("should save when responsible user changes", function () {
            // arrange
            var responsibleSelect = pageObject.getResponsibleSelect(0);
            // act
            responsibleSelect.then(function (selector) { return selector.selectFirst("2"); });
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/communication/" });
        });
    });
});
