import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-status-modal.po");

describe("project edit tab staus modal", () => {
    var mockHelper: Helper.Mock;
    var pageObject: PageObject;
    var mockDependencies: Array<string> = [
        "itproject",
        "itprojecttype",
        "itprojectright",
        "itprojectrole",
        "itprojectstatus"
    ];

    beforeEach(() => {
        browser.driver.manage().window().maximize();

        mockHelper = new Helper.Mock();
        pageObject = new PageObject();
    });

    afterEach(() => {
        mock.teardown();
    });

    describe("assignment", () => {
        describe("with no write access", () => {
            beforeEach(done => {
                mock(["assignment", "itProjectNoWriteAccess", "assignmentNoWriteAccess"].concat(mockDependencies));
                pageObject.getAssignmentPage()
                    .then(() => mock.clearRequests())
                    .then(() => done());
            });

            it("should disable inputs", () => {
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

            it("should hide save and cancel buttons", () => {
                // arrange

                // act

                // assert
                expect(pageObject.saveButton).not.toBeVisible();
                expect(pageObject.cancelButton).not.toBeVisible();
            });
        });

        describe("with write access", () => {
            beforeEach(done => {
                mock(["assignment", "itProjectWriteAccess", "assignmentWriteAccess"].concat(mockDependencies));
                pageObject.getAssignmentPage()
                    .then(() => mock.clearRequests())
                    .then(() => done());
            });

            it("should save on save click", () => {
                // arrange

                // act
                pageObject.saveButton.click();

                // assert
                expect(mock.requestsMade()).toMatchInRequests({ method: "PATCH", url: "api/assignment/1" });
            });

            it("should not save on cancel click", () => {
                // arrange

                // act
                pageObject.cancelButton.click();

                // assert
                expect(mock.requestsMade()).not.toMatchInRequests({ method: "POST", url: "api/assignment/" });
            });

            it("should hide close buttons", () => {
                // arrange

                // act

                // assert
                expect(pageObject.closeButton).not.toBeVisible();
            });
        });
    });

    describe("milestone", () => {
        describe("with no write access", () => {
            beforeEach(() => {
                mock(["milestone", "itProjectNoWriteAccess", "milestoneNoWriteAccess"].concat(mockDependencies));
                pageObject.getMilestonePage();

                // clear initial requests
                mock.clearRequests();
            });

            it("should disable inputs", () => {
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

            it("should show date", () => {
                // arrange

                // act

                // assert
                expect(pageObject.dateElement).toBeVisible();
            });
        });
    });
});
