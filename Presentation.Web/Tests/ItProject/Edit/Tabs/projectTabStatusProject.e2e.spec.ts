import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-status-project.po");

describe("project edit tab status project", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: PageObject;
    var mockDependencies: Array<string> = [
        "itproject",
        "itprojectrole",
        "itprojecttype",
        "itprojectright",
        "itprojectstatus",
        "assignment"
    ];

    beforeEach(() => {
        browser.driver.manage().window().maximize();

        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);
        pageObject = new PageObject();
    });

    afterEach(() => {
        mock.teardown();
    });

    describe("with no write access", () => {
        beforeEach(done => {
            mock(["itProjectNoWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should disable inputs", () => {
            // arrange

            // act

            // assert
            expect(pageObject.statusTrafficLightSelect.isDisabled()).toBeTruthy("traffic light is active");
            expect(pageObject.statusUpdateDateElement).toBeDisabled();
            expect(pageObject.statusNoteElement).toBeDisabled();
        });

        // TODO: Modal closes unexpectedly right after click
        //it("should show view activity", () => {
        //    // arrange
        //
        //    // act
        //    pageObject.assignmentMilestoneRepeater.selectFirst("a.view-activity").first().click();
        //
        //    // assert
        //    expect(browser.driver.getCurrentUrl()).toMatch("modal/assignment/");
        //});
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should save when status traffic light changes", () => {
            // arrange

            // act
            pageObject.statusTrafficLightSelect.select(1);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should save when status update date looses focus", () => {
            // arrange
            pageObject.statusUpdateInput("01-01-2015");

            // act
            pageObject.statusNoteElement.click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should save when status note looses focus", () => {
            // arrange
            pageObject.statusNoteInput("SomeNote");

            // act
            pageObject.statusUpdateDateElement.click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should present assignments and milestones in table on load", () => {
            // arrange

            // act

            // assert
            expect(pageObject.assignmentMilestoneRepeater.count()).toBe(2);
        });

        // TODO: Modal closes unexpectedly right after click
        //it("should open edit modal when item name is clicked", () => {
        //    // arrange

        //    // act
        //    pageObject.assignmentMilestoneRepeater.selectFirst("a.edit-activity").first().click();

        //    // assert
        //    expect(browser.driver.getCurrentUrl()).toMatch("modal/assignment/2311");
        //});

        it("should delete activity when delete confirm popup is accepted", () => {
            // arrange
            pageObject.assignmentMilestoneRepeater
                .selectFirst(pageObject.assigmentLocator)
                .first()
                .click();

            // act
            browserHelper.acceptAlert();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/Assignment/1" });
        });

        it("should not delete activity when delete confirm popup is dismissed", () => {
            // arrange
            pageObject.assignmentMilestoneRepeater
                .selectFirst(pageObject.assigmentLocator)
                .first()
                .click();

            // act
            browserHelper.dismissAlert();

            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/Assignment/1" });
        });

        it("should open assignment modal when add assignment is clicked", () => {
            // arrange

            // act
            pageObject.addAssignmentButton.click();

            // assert
            expect(browser.driver.getCurrentUrl()).toMatch("modal/assignment/");
        });

        it("should open assignment modal when add assignment is clicked", () => {
            // arrange

            // act
            pageObject.addAssignmentButton.click();

            // assert
            expect(browser.driver.getCurrentUrl()).toMatch("modal/assignment/");
        });

        it("should open milestone modal when add milestone is clicked", () => {
            // arrange

            // act
            pageObject.addMilestoneButton.click();

            // assert
            expect(browser.driver.getCurrentUrl()).toMatch("modal/milestone/");
        });
    });
});
