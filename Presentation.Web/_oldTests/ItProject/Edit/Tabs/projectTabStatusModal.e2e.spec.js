"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../../helper");
var PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-status-modal.po");
describe("project edit tab staus modal", function () {
    var mockHelper;
    var pageObject;
    var mockDependencies = [
        "itproject",
        "itprojecttype",
        "itprojectright",
        "itprojectrole",
        "itprojectstatus"
    ];
    beforeEach(function () {
        browser.driver.manage().window().maximize();
        mockHelper = new Helper.Mock();
        pageObject = new PageObject();
    });
    afterEach(function () {
        mock.teardown();
    });
    describe("assignment", function () {
        describe("with no write access", function () {
            beforeEach(function (done) {
                mock(["assignment", "itProjectNoWriteAccess", "assignmentNoWriteAccess"].concat(mockDependencies));
                pageObject.getAssignmentPage()
                    .then(function () { return mock.clearRequests(); })
                    .then(function () { return done(); });
            });
            it("should disable inputs", function () {
                // arrange
                // act
                // assert
                expect(pageObject.nameElement).toBeDisabled();
                expect(pageObject.humanReadableIdElement).toBeDisabled();
                expect(pageObject.phaseSelector).toBeDisabled();
                expect(pageObject.startDateElement).toBeDisabled();
                expect(pageObject.endDateElement).toBeDisabled();
                expect(pageObject.timeEstimateElement).toBeDisabled();
                expect(pageObject.statusSelector).toBeDisabled();
                expect(pageObject.descriptionElement).toBeDisabled();
                expect(pageObject.noteElement).toBeDisabled();
                expect(pageObject.associatedUserElement).toBeDisabled();
            });
            it("should hide save and cancel buttons", function () {
                // arrange
                // act
                // assert
                expect(pageObject.saveButton).not.toBeVisible();
                expect(pageObject.cancelButton).not.toBeVisible();
            });
        });
        describe("with write access", function () {
            beforeEach(function (done) {
                mock(["assignment", "itProjectWriteAccess", "assignmentWriteAccess"].concat(mockDependencies));
                pageObject.getAssignmentPage()
                    .then(function () { return mock.clearRequests(); })
                    .then(function () { return done(); });
            });
            it("should save on save click", function () {
                // arrange
                // act
                pageObject.saveButton.click();
                // assert
                expect(mock.requestsMade()).toMatchInRequests({ method: "PATCH", url: "api/assignment/1" });
            });
            it("should not save on cancel click", function () {
                // arrange
                // act
                pageObject.cancelButton.click();
                // assert
                expect(mock.requestsMade()).not.toMatchInRequests({ method: "POST", url: "api/assignment/" });
            });
            it("should hide close buttons", function () {
                // arrange
                // act
                // assert
                expect(pageObject.closeButton).not.toBeVisible();
            });
        });
    });
    describe("milestone", function () {
        describe("with no write access", function () {
            beforeEach(function () {
                mock(["milestone", "itProjectNoWriteAccess", "milestoneNoWriteAccess"].concat(mockDependencies));
                pageObject.getMilestonePage();
                // clear initial requests
                mock.clearRequests();
            });
            it("should disable inputs", function () {
                // arrange
                // act
                // assert
                expect(pageObject.nameElement).toBeDisabled();
                expect(pageObject.humanReadableIdElement).toBeDisabled();
                expect(pageObject.phaseSelector).toBeDisabled();
                expect(pageObject.timeEstimateElement).toBeDisabled();
                expect(pageObject.milestoneStatusSelector.isDisabled()).toBeTrue();
                expect(pageObject.descriptionElement).toBeDisabled();
                expect(pageObject.noteElement).toBeDisabled();
                expect(pageObject.associatedUserElement).toBeDisabled();
            });
            it("should show date", function () {
                // arrange
                // act
                // assert
                expect(pageObject.dateElement).toBeVisible();
            });
        });
    });
});
