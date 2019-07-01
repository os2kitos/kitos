"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../../helper");
var PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-stakeholders.po");
describe("project edit tab stakeholders", function () {
    var mockHelper;
    var browserHelper;
    var pageObject;
    var mockDependencies = ["itproject", "itprojecttype", "itprojectstatus", "stakeholder"];
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
            expect(pageObject.nameElement.isPresent()).toBeFalse();
            expect(pageObject.roleElement.isPresent()).toBeFalse();
            expect(pageObject.downsidesElement.isPresent()).toBeFalse();
            expect(pageObject.benefitsElement.isPresent()).toBeFalse();
            expect(pageObject.significanceElement.isPresent()).toBeFalse();
            expect(pageObject.howToHandleElement.isPresent()).toBeFalse();
            expect(pageObject.saveStakeholderElement.isPresent()).toBeFalse();
        });
        it("should disable repeated stakeholder input fields", function () {
            // arrange
            // act
            // assert
            expect(pageObject.stakeholderRepeater.selectFirst(pageObject.nameLocator).first()).toBeDisabled();
            expect(pageObject.stakeholderRepeater.selectFirst(pageObject.roleLocator).first()).toBeDisabled();
            expect(pageObject.stakeholderRepeater.selectFirst(pageObject.downsidesLocator).first()).toBeDisabled();
            expect(pageObject.stakeholderRepeater.selectFirst(pageObject.benefitsLocator).first()).toBeDisabled();
            expect(pageObject.stakeholderRepeater.selectFirst(pageObject.significanceLocator).first()).toBeDisabled();
            expect(pageObject.stakeholderRepeater.selectFirst(pageObject.howToHandleLocator).first()).toBeDisabled();
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should mark inputs required on save when nothing is entered", function () {
            // arrange
            // act
            pageObject.saveStakeholderElement.click()
                .then(function () {
                // assert
                expect(pageObject.nameElement.element(by.xpath(".."))).toHaveClass("has-error");
                expect(pageObject.roleElement.element(by.xpath(".."))).toHaveClass("has-error");
                expect(pageObject.downsidesElement.element(by.xpath(".."))).toHaveClass("has-error");
                expect(pageObject.benefitsElement.element(by.xpath(".."))).toHaveClass("has-error");
                expect(pageObject.significanceElement.element(by.xpath(".."))).toHaveClass("has-error");
                expect(pageObject.howToHandleElement.element(by.xpath(".."))).toHaveClass("has-error");
            });
        });
        it("should save when save is clicked", function () {
            // arrange
            pageObject.nameInput("SomeName");
            pageObject.roleInput("SomeRole");
            pageObject.downsidesInput("SomeDownside");
            pageObject.benefitsInput("SomeBenifit");
            pageObject.significanceInput(1);
            pageObject.howToHandleInput("SomeHandling");
            // act
            pageObject.saveStakeholderElement.click()
                .then(function () {
                // assert
                expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/stakeholder" });
            });
        });
        it("should repeat stakeholders", function () {
            // arrange
            // act
            // assert
            expect(pageObject.stakeholderRepeater.count()).toBeGreaterThan(0);
        });
        it("should delete stakeholder when delete is confirmed", function () {
            // arrange
            pageObject.stakeholderRepeater
                .selectFirst(pageObject.deleteStakeholderLocator)
                .first()
                .click();
            // act
            browserHelper.acceptAlert();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/stakeholder" });
        });
        it("should not delete stakeholder when delete is dismissed", function () {
            // arrange
            pageObject.stakeholderRepeater
                .selectFirst(pageObject.deleteStakeholderLocator)
                .first()
                .click();
            // act
            browserHelper.dismissAlert();
            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/stakeholder" });
        });
        it("should save when name looses focus", function () {
            // arrange
            // act
            pageObject.stakeholderRepeater
                .selectFirst(pageObject.nameLocator)
                .sendKeys("NewName", protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/stakeholder" });
        });
        it("should save when role looses focus", function () {
            // arrange
            mock.clearRequests();
            // act
            pageObject.stakeholderRepeater
                .selectFirst(pageObject.roleLocator)
                .sendKeys("NewRole", protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/stakeholder" });
        });
        it("should save when downsides looses focus", function () {
            // arrange
            // act
            pageObject.stakeholderRepeater
                .selectFirst(pageObject.downsidesLocator)
                .sendKeys("NewDownside", protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/stakeholder" });
        });
        it("should save when benefits looses focus", function () {
            // arrange
            // act
            pageObject.stakeholderRepeater
                .selectFirst(pageObject.benefitsLocator)
                .sendKeys("NewBenefit", protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/stakeholder" });
        });
        it("should save when significance looses focus", function () {
            // arrange
            // act
            pageObject.stakeholderRepeater
                .selectFirst(pageObject.significanceLocator)
                .sendKeys(protractor.Key.CONTROL, "a", protractor.Key.NULL, "5", protractor.Key.TAB); // CTRL + a first to clear old value
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/stakeholder" });
        });
        it("should save when hot to handle looses focus", function () {
            // arrange
            // act
            pageObject.stakeholderRepeater
                .selectFirst(pageObject.howToHandleLocator)
                .sendKeys("NewHandling", protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/stakeholder" });
        });
    });
});
