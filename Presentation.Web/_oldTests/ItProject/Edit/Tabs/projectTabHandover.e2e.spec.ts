import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-handover.po");

describe("project edit tab handover", () => {
    var mockHelper: Helper.Mock;
    var pageObject: PageObject;
    var mockDependencies: Array<string> = [
        "itproject",
        "itprojecttype",
        "itprojectrole",
        "itprojectright",
        "handover"
    ];

    beforeEach(() => {
        browser.driver.manage().window().maximize();

        mockHelper = new Helper.Mock();
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
            expect(pageObject.descriptionElement).toBeDisabled();
            expect(pageObject.meetingElement).toBeDisabled();
            expect(pageObject.participantsSelect.element).toBeSelect2Disabled();
            expect(pageObject.summaryElement).toBeDisabled();
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should save when description looses focus", () => {
            // arrange
            pageObject.descriptionInput("SomeDescription");

            // act
            pageObject.descriptionElement.sendKeys(protractor.Key.TAB);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/handover/1" });
        });

        it("should save when meeting data looses focus", () => {
            // arrange
            pageObject.meetingInput("01-01-2017");

            // act
            pageObject.meetingElement.sendKeys(protractor.Key.TAB);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/handover/1" });
        });

        it("should save when participants changes", () => {
            // arrange

            // act
            pageObject.participantsSelect.selectFirst();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "POST", url: "api/handover/1" });
        });

        it("should save when summary changes", () => {
            // arrange
            pageObject.summaryInput("SomeDescription");

            // act
            pageObject.summaryElement.sendKeys(protractor.Key.TAB);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/handover/1" });
        });

        it("should add participant to tags when clicked", () => {
            // arrange

            // act
            pageObject.participantsSelect.selectFirst();

            // assert
            expect(pageObject.participantsSelect.selectedOptions().count()).not.toBe(0);
        });
    });
});
