import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-status-goal.po");

describe("project edit tab status goal", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: PageObject;
    var mockDependencies: Array<string> = ["itproject", "itprojecttype", "goalType", "goalStatus", "goal"];

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
            expect(pageObject.trafficLightSelect.isDisabled()).toBeTruthy("traffic light is active");
            expect(pageObject.updateDateElement).toBeDisabled();
            expect(pageObject.noteElement).toBeDisabled();
        });

        it("should hide goal links", () => {
            // arrange
            // act

            // assert
            expect(element(pageObject.editGoalLocator)).not.toBeVisible();
            expect(element(pageObject.deleteGoalLocator)).not.toBeVisible();
            expect(pageObject.addGoalButton).not.toBeVisible();
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should save when traffic light changes", () => {
            // arrange

            // act
            pageObject.trafficLightSelect.select(1);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/goalStatus" });
        });

        it("should save when update date looses focus", () => {
            // arrange
            pageObject.updateInput("01-01-2015");

            // act
            pageObject.updateDateElement.sendKeys(protractor.Key.TAB);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/goalStatus" });
        });

        it("should save when note looses focus", () => {
            // arrange
            pageObject.noteInput("SomeNote");

            // act
            pageObject.noteElement.sendKeys(protractor.Key.TAB);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/goalStatu" });
        });

        it("should present goals in table on load", () => {
            // arrange

            // act

            // assert
            expect(pageObject.goalRepeater.count()).toBeGreaterThan(0);
        });

        //// TODO: model closes right after click. Using ui-sref on link fixes issue.
        //it("should open edit modal when edit is clicked", () => {
        //    // arrange

        //    // act
        //    element(pageObject.editGoalLocator).click();

        //    // assert
        //    expect(browser.driver.getCurrentUrl()).toMatch("status-goal/modal/");
        //});

        it("should delete goal when delete confirm popup is accepted", () => {
            // arrange
            pageObject.goalRepeater
                .selectFirst(pageObject.deleteGoalLocator)
                .first()
                .click();

            // act
            browserHelper.acceptAlert();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/goal" });
        });

        it("should not delete goal when delete confirm popup is dismissed", () => {
            // arrange
            pageObject.goalRepeater
                .selectFirst(pageObject.deleteGoalLocator)
                .first()
                .click();

            // act
            browserHelper.dismissAlert();

            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/goal" });
        });

        it("should open goal modal when add goal is clicked", () => {
            // arrange

            // act
            pageObject.addGoalButton.click();

            // assert
            expect(browser.driver.getCurrentUrl()).toMatch("status-goal/modal");
        });
    });
});
