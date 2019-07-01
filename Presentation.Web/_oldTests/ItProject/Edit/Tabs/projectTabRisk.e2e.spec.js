"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../../helper");
var PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-risk.po");
describe("project edit tab risk", function () {
    var mockHelper;
    var browserHelper;
    var pageObject;
    var mockDependencies = [
        "itproject",
        "itprojecttype",
        "itprojectright",
        "itprojectrole",
        "itprojectstatus",
        "risk"
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
            expect(pageObject.nameElement).not.toBeVisible();
            expect(pageObject.consequenceElement).not.toBeVisible();
            expect(pageObject.probabilityElement).not.toBeVisible();
            expect(pageObject.actionElement).not.toBeVisible();
            expect(pageObject.responsibleSelect.element).not.toBeVisible();
        });
        it("should disable repeated risk input fields", function () {
            // arrange
            // act
            // assert
            expect(pageObject.riskRepeater.selectFirst(pageObject.nameLocator).first()).toBeDisabled();
            expect(pageObject.riskRepeater.selectFirst(pageObject.consequenceLocator).first()).toBeDisabled();
            expect(pageObject.riskRepeater.selectFirst(pageObject.probabilityLocator).first()).toBeDisabled();
            expect(pageObject.riskRepeater.selectFirst(pageObject.actionLocator).first()).toBeDisabled();
            expect(pageObject.riskRepeater.selectFirst(pageObject.deleteRiskLocator).first()).toBeDisabled();
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
        it("should mark inputs required on save when nothing is entered", function () {
            // arrange
            // act
            pageObject.saveRiskElement.click()
                .then(function () {
                // assert
                expect(pageObject.nameElement.element(by.xpath(".."))).toHaveClass("has-error");
                expect(pageObject.actionElement.element(by.xpath(".."))).toHaveClass("has-error");
                expect(pageObject.responsibleSelect.element.element(by.xpath(".."))).toHaveClass("has-error");
            });
        });
        xit("should save when save is clicked", function () {
            // arrange
            // below is dummy. Hardcoded values are returned from mock response
            pageObject.nameInput("SomeName");
            pageObject.consequenceInput(2);
            pageObject.probabilityInput(2);
            pageObject.actionInput("SomeAction");
            pageObject.responsibleSelect.selectFirst();
            // act
            pageObject.saveRiskElement.click()
                .then(function () {
                // assert
                expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/risk" });
            });
        });
        it("should repeat risks", function () {
            // arrange
            // act
            // assert
            expect(pageObject.riskRepeater.count()).toBeGreaterThan(0);
        });
        it("should delete risk when delete is confirmed", function () {
            // arrange
            pageObject.riskRepeater
                .selectFirst(pageObject.deleteRiskLocator)
                .first()
                .click();
            // act
            browserHelper.acceptAlert();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/risk" });
        });
        it("should not delete risk when delete is dismissed", function () {
            // arrange
            pageObject.riskRepeater
                .selectFirst(pageObject.deleteRiskLocator)
                .first()
                .click();
            // act
            browserHelper.dismissAlert();
            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/risk" });
        });
        it("should save when name looses focus", function () {
            // arrange
            // act
            pageObject.riskRepeater
                .selectFirst(pageObject.nameLocator)
                .sendKeys("NewName", protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/risk" });
        });
        it("should save when consequence looses focus", function () {
            // arrange
            // act
            pageObject.riskRepeater
                .selectFirst(pageObject.consequenceLocator)
                .sendKeys(protractor.Key.CONTROL, "a", protractor.Key.NULL, "2", protractor.Key.TAB); // CTRL + a to clear input
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/risk" });
        });
        it("should save when probability looses focus", function () {
            // arrange
            // act
            pageObject.riskRepeater
                .selectFirst(pageObject.probabilityLocator)
                .sendKeys(protractor.Key.CONTROL, "a", protractor.Key.NULL, "2", protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/risk" });
        });
        it("should save when action looses focus", function () {
            // arrange
            // act
            pageObject.riskRepeater
                .selectFirst(pageObject.actionLocator)
                .sendKeys("NewAction", protractor.Key.TAB);
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/risk" });
        });
        it("should save when responsible user changes", function () {
            // arrange
            var responsibleSelect = pageObject.getResponsibleSelect(0);
            // act
            responsibleSelect.then(function (selector) { return selector.selectFirst("2"); });
            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/risk" });
        });
        it("should calculate consequence and probability product on excisting risk", function () {
            // arrange
            // act
            pageObject.consequenceInput(2);
            pageObject.probabilityInput(2);
            pageObject.riskRepeater
                .selectFirst(pageObject.consequenceLocator)
                .sendKeys(protractor.Key.CONTROL, "a", protractor.Key.NULL, "2", protractor.Key.TAB); // CTRL + a to clear input
            pageObject.riskRepeater
                .selectFirst(pageObject.probabilityLocator)
                .sendKeys(protractor.Key.CONTROL, "a", protractor.Key.NULL, "2", protractor.Key.TAB);
            // assert
            var value = pageObject.riskRepeater
                .selectFirst(pageObject.productLocator)
                .first()
                .getInnerHtml();
            expect(value).toBe("4");
        });
        it("should calculate consequence and probability product on new risk", function () {
            // arrange
            // act
            pageObject.consequenceInput(2);
            pageObject.probabilityInput(2);
            // assert
            expect(pageObject.productValue()).toBe("4");
        });
        xit("should calculate average consequence and probability product on risks", function () {
            // arrange
            // excisting risk has consequence and probability of 1
            // below is dummy. Hardcoded values are returned from mock response
            pageObject.nameInput("SomeName");
            pageObject.consequenceInput(2);
            pageObject.probabilityInput(2);
            pageObject.actionInput("SomeAction");
            pageObject.responsibleSelect.selectFirst();
            var expectedAverage = (2 * 2 + 1 * 1) / 2;
            // act
            pageObject.saveRiskElement.click();
            // assert
            expect(pageObject.averageProductValue()).toBe(expectedAverage.toString());
        });
    });
});
