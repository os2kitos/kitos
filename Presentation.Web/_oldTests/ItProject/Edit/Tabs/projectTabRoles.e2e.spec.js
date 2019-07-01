"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var mock = require("protractor-http-mock");
var Helper = require("../../../helper");
var PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-roles.po");
describe("project edit tab roles", function () {
    var mockHelper;
    var browserHelper;
    var pageObject;
    var mockDependencies = [
        "itproject",
        "itprojectrole",
        "itprojecttype",
        "itprojectright",
        "itprojectstatus",
        "assignment",
        "organization"
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
        it("should hide new right fields", function () {
            // arrange
            // act
            // assert
            expect(pageObject.addRightRoleSelector.element).not.toBeVisible();
            expect(pageObject.addRightUserSelector.element).not.toBeVisible();
        });
        it("should hide edit and delete buttons on rights", function () {
            // arrange
            var editButton = pageObject.rightsRepeater
                .selectFirst(pageObject.rightEditButtonLocator)
                .first();
            var deleteButton = pageObject.rightsRepeater
                .selectFirst(pageObject.rightEditDeleteLocator)
                .first();
            // act
            // assert
            expect(editButton).not.toBeVisible();
            expect(deleteButton).not.toBeVisible();
        });
    });
    describe("with write access", function () {
        beforeEach(function (done) {
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(function () { return mock.clearRequests(); })
                .then(function () { return done(); });
        });
        it("should init roles selector", function () {
            // arrange
            var element = pageObject.addRightRoleSelector;
            // act
            element.element.click();
            // assert
            expect(element.options.count()).toBeGreaterThan(0, "No options in selector");
        });
        it("should get users when typing", function () {
            // arrange
            var element = pageObject.addRightUserSelector;
            // act
            element.selectFirst("t");
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "GET", url: "api/organization" });
        });
        it("should save right when user is selected", function () {
            // arrange
            var element = pageObject.addRightUserSelector;
            // act
            element.selectFirst("t");
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/itprojectright" });
        });
        it("should repeat rights", function () {
            // arrange
            // act
            // assert
            // two rows are created per right, one for display one for edit
            var count = pageObject.rightsRepeater.selectFirst(pageObject.rightRowLocator).count();
            expect(count).toBe(2);
        });
        it("should display edit field for rights when edit is clicked", function () {
            // arrange
            // act
            pageObject.rightsRepeater
                .selectFirst(pageObject.rightEditButtonLocator)
                .click();
            // assert
            var count = pageObject.rightsRepeater.selectFirst(pageObject.rightEditRoleInputLocator).count();
            expect(count).toBe(1);
        });
        it("should save edited field for rights when save is clicked", function () {
            // arrange
            pageObject.rightsRepeater
                .selectFirst(pageObject.rightEditButtonLocator)
                .click();
            mock.clearRequests();
            // act
            pageObject.rightsRepeater
                .selectFirst(pageObject.rightEditSaveButtonLocator)
                .click();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itprojectright" });
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/itprojectright" });
        });
        it("should delete right when delete confirmed", function () {
            // arrange
            mock.clearRequests();
            pageObject.rightsRepeater
                .selectFirst(pageObject.rightEditDeleteLocator)
                .click();
            // act
            browserHelper.acceptAlert();
            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itprojectright" });
        });
        it("should not delete right when delete dismissed", function () {
            // arrange
            pageObject.rightsRepeater
                .selectFirst(pageObject.rightEditDeleteLocator)
                .click();
            // act
            browserHelper.dismissAlert();
            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itprojectright" });
        });
    });
});
