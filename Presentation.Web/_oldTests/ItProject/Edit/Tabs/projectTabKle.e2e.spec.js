"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../../helper");
var PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-kle.po");
describe("project edit tab kle", function () {
    var mockHelper;
    var browserHelper;
    var pageObject;
    var mockDependencies = ["itproject", "itprojecttype", "taskref"];
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
        it("should disable all checkboxes", function () {
            // arrange
            // act
            // assert
            pageObject.taskRepeater.each(function (elem) {
                var checkbox = elem.element(pageObject.checkboxLocator);
                expect(checkbox).toBeDisabled();
            });
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            // TODO: itproject/1/taskId=* is hardcoded in mock JSON for IDs 19-68. Refactor to object based mock generation.
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should save when task is checked", function () {
            // arrange
            // act
            pageObject.taskRepeater.selectFirst(pageObject.checkboxLocator).first().click()
                .then(function () {
                // assert
                expect(mockHelper.lastRequest()).toMatchRequest({ method: "POST", url: "api/itProject/1" });
            });
        });
        it("should repeat tasks", function () {
            // arrange
            // act
            // assert
            expect(pageObject.taskRepeater.count()).toBeGreaterThan(0);
        });
        it("should disable group selector when main group is not selected", function () {
            // arrange
            // act
            // catch error to ignore nothing selected error
            pageObject.mainGroupSelect.deselect().thenCatch(function () { return null; });
            // assert
            expect(pageObject.groupSelect.element).toBeSelect2Disabled();
        });
        it("should enable group selector when main group is selected", function () {
            // arrange
            // act
            pageObject.mainGroupSelect.selectFirst();
            // assert
            expect(pageObject.groupSelect.element).not.toBeSelect2Disabled();
        });
        it("should get tasks when main group is selected", function () {
            // arrange
            // act
            pageObject.mainGroupSelect.selectFirst();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "GET", url: "api/itProject/1?(.)*taskGroup=[0-9]" });
        });
        it("should get tasks when group is selected", function () {
            // arrange
            pageObject.mainGroupSelect.selectFirst();
            mock.clearRequests();
            // act
            pageObject.groupSelect.selectFirst();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "GET", url: "api/itProject/1?(.)*taskGroup=[0-9]" });
        });
        it("should get selected tasks only on show selected click", function () {
            // arrange
            // act
            pageObject.changeTaskViewElement.click();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "GET", url: "api/itProject/1?(.)*tasks=true(.)*onlySelected=true" });
        });
        it("should check all tasks on confirmed select all pages click", function () {
            // arrange
            pageObject.selectAllPagesElement.click();
            // act
            browserHelper.acceptAlert();
            // assert
            // match api url with query string 'taskID=' only and ignoring any other query parameters
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/itProject/1?(.[^ ])*taskId=[^0-9]" });
        });
        it("should not check all tasks on dismissed select all pages click", function () {
            // arrange
            pageObject.selectAllPagesElement.click();
            // act
            browserHelper.dismissAlert();
            // assert
            // match api url with query string 'taskID=' only and ignoring any other query parameters
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "POST", url: "api/itProject/1?(.[^ ])*taskId=[^0-9]" });
        });
        it("should check all tasks on page on confirmed select all click", function () {
            // arrange
            pageObject.selectAllElement.click();
            // act
            browserHelper.acceptAlert();
            // assert
            pageObject.taskRepeater.each(function (elem) {
                var checkbox = elem.element(pageObject.checkboxLocator);
                // match api url with query string 'taskID=ID' only and ignoring any other query parameters
                checkbox.getAttribute("id")
                    .then(function (id) {
                    return expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/itProject/1?(.[^ ])*taskId=" + id });
                });
                expect(checkbox).toBeChecked();
            });
        });
        it("should not check all tasks on page on dismissed select all click", function () {
            // arrange
            pageObject.selectAllElement.click();
            // act
            browserHelper.dismissAlert();
            // assert
            pageObject.taskRepeater.each(function (elem) {
                var checkbox = elem.element(pageObject.checkboxLocator);
                // match api url with query string 'taskID=ID' only and ignoring any other query parameters
                checkbox.getAttribute("id")
                    .then(function (id) {
                    return expect(mock.requestsMade()).not.toMatchInRequests({ method: "POST", url: "api/itProject/1?(.[^ ])*taskId=" + id });
                });
                expect(checkbox).not.toBeChecked();
            });
        });
        it("should uncheck all tasks on page on confirmed deselect all click", function () {
            // arrange
            pageObject.selectAllElement.click();
            browserHelper.acceptAlert();
            mock.clearRequests();
            pageObject.deselectAllElement.click();
            // act
            browserHelper.acceptAlert();
            // assert
            pageObject.taskRepeater.each(function (elem) {
                var checkbox = elem.element(pageObject.checkboxLocator);
                // match api url with query string 'taskID=ID' only and ignoring any other query parameters
                checkbox.getAttribute("id")
                    .then(function (id) {
                    return expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itProject/1?(.[^ ])*taskId=" + id });
                });
                expect(checkbox).not.toBeChecked();
            });
        });
        it("should not uncheck all tasks on page on dismissed deselect all click", function () {
            // arrange
            pageObject.selectAllElement.click();
            browserHelper.acceptAlert();
            mock.clearRequests();
            pageObject.deselectAllElement.click();
            // act
            browserHelper.dismissAlert();
            // assert
            pageObject.taskRepeater.each(function (elem) {
                var checkbox = elem.element(pageObject.checkboxLocator);
                // match api url with query string 'taskID=ID' only and ignoring any other query parameters
                checkbox.getAttribute("id")
                    .then(function (id) {
                    return expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itProject/1?(.[^ ])*taskId=" + id });
                });
                expect(checkbox).toBeChecked();
            });
        });
        it("should uncheck all tasks on confirmed deselect all pages click", function () {
            // arrange
            pageObject.selectAllElement.click();
            browserHelper.acceptAlert();
            mock.clearRequests();
            pageObject.deselectAllPages.click();
            // act
            browserHelper.acceptAlert();
            // assert
            // match api url with query string 'taskID=' only and ignoring any other query parameters
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itProject/1?(.[^ ])*taskId=[^0-9]" });
        });
        it("should not uncheck all tasks on dismissed deselect all pages click", function () {
            // arrange
            pageObject.selectAllElement.click();
            browserHelper.acceptAlert();
            mock.clearRequests();
            pageObject.deselectAllPages.click();
            // act
            browserHelper.dismissAlert();
            // assert
            // match api url with query string 'taskID=' only and ignoring any other query parameters
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itProject/1?(.[^ ])*taskId=[^0-9]" });
        });
    });
});
