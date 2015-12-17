import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-status-project.po");

describe("project edit tab status project", () => {
    var mockHelper: Helper.Mock;
    var pageObject: PageObject;

    beforeEach(() => {
        mock(["itproject", "itprojectrole", "itprojecttype", "itprojectrights", "itprojectstatus", "assignment"]);

        mockHelper = new Helper.Mock();

        pageObject = new PageObject();
        pageObject.getPage();

        browser.driver.manage().window().maximize();

        // clear initial requests
        mock.clearRequests();
    });

    afterEach(() => {
        mock.teardown();
    });

    it("should save when project status changes", () => {
        // arrange

        // act
        pageObject.statusTrafficLightSelect.select(1);

        // assert
        expect(mockHelper.lastRequest({ method: "PATCH", url: "api/itproject/1" })).toBeTruthy();
    });

    it("should save when status update date looses focus", () => {
        // arrange
        pageObject.statusUpdateInput = "01-01-2015";

        // act
        pageObject.statusNoteElement.click();

        // assert
        expect(mockHelper.lastRequest({ method: "PATCH", url: "api/itproject/1" })).toBeTruthy();
    });

    it("should save when status note looses focus", () => {
        // arrange
        pageObject.statusNoteInput = "SomeNote";

        // act
        pageObject.statusUpdateDateElement.click();

        // assert
        expect(mockHelper.lastRequest({ method: "PATCH", url: "api/itproject/1" })).toBeTruthy();
    });

    it("should present assignments and milestones in table on load", () => {
        // arrange

        // act

        // assert
        expect(pageObject.assignmentMilestoneRepeater.repeater.count()).toBe(2);
    });

    // TODO: Some issue with the modal closing after initiation
    //it("should open edit modal when item name is clicked", () => {
    //    // arrange

    //    // act
    //    pageObject.assignmentMilestoneRepeater.selectFirst("a.edit-activity").first().click();

    //    // assert
    //    expect(browser.driver.getCurrentUrl()).toMatch("modal/assignment/2311");
    //});

    it("should delete activity when delete confirm popup is accepted", () => {
        // arrange
        pageObject.assignmentMilestoneRepeater.selectFirst("a.delete-activity").first().click();

        // act
        browser.switchTo().alert()
            .then(alert => alert.accept());

        // assert
        expect(mockHelper.findRequest({ method: "DELETE", url: "api/Assignment/1" })).toBeTruthy("DELETE HTTP request not detected");
    });

    it("should not delete activity when delete confirm popup is dismissed", () => {
        // arrange
        pageObject.assignmentMilestoneRepeater.selectFirst("a.delete-activity").first().click();

        // act
        browser.switchTo().alert()
            .then(alert => alert.dismiss());

        // assert
        expect(mockHelper.findRequest({ method: "DELETE", url: "api/Assignment/1" })).toBeFalsy("DELETE HTTP request detected");
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
