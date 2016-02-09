import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-communication.po");

describe("project edit tab communication", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: PageObject;
    var mockDependencies: Array<string> = [
        "itproject",
        "itprojecttype",
        "itprojectright",
        "itprojectrole",
        "itprojectstatus",
        "communication"
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

        it("should hide input fields", () => {
            // arrange

            // act

            // assert
            expect(pageObject.targetElement).not.toBeVisible();
            expect(pageObject.purposeElement).not.toBeVisible();
            expect(pageObject.messageElement).not.toBeVisible();
            expect(pageObject.mediaElement).not.toBeVisible();
            expect(pageObject.dueDateElement).not.toBeVisible();
            expect(pageObject.responsibleSelect.element).not.toBeVisible();
        });

        it("should disable repeated comm input fields", () => {
            // arrange

            // act

            // assert
            expect(pageObject.commRepeater.selectFirst(pageObject.targetLocator).first()).toBeDisabled();
            expect(pageObject.commRepeater.selectFirst(pageObject.purposeLocator).first()).toBeDisabled();
            expect(pageObject.commRepeater.selectFirst(pageObject.messageLocator).first()).toBeDisabled();
            expect(pageObject.commRepeater.selectFirst(pageObject.mediaLocator).first()).toBeDisabled();
            expect(pageObject.commRepeater.selectFirst(pageObject.dueDateLocator).first()).toBeDisabled();
            pageObject.getResponsibleSelect(0).then(s => expect(s.element).toBeSelect2Disabled());
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should mark required inputs on save when nothing is entered", () => {
            // arrange

            // act
            pageObject.saveCommElement.click()
                .then(() => {

                    // assert
                    expect(pageObject.dueDateElement.element(by.xpath("../../.."))).toHaveClass("has-error");
                    expect(pageObject.responsibleSelect.element.element(by.xpath(".."))).toHaveClass("has-error");
                });
        });

        it("should save when save is clicked", () => {
            // arrange
            // below is dummy. Hardcoded values are returned from mock response
            pageObject.dueDateInput("01-01-2016");
            pageObject.responsibleSelect.selectFirst();

            // act
            pageObject.saveCommElement.click()
                .then(() => {

                    // assert
                    expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/communication" });
                });
        });

        it("should repeat comms", () => {
            // arrange

            // act

            // assert
            expect(pageObject.commRepeater.count()).toBeGreaterThan(0);
        });

        it("should delete comm when delete is confirmed", () => {
            // arrange
            pageObject.commRepeater
                .selectFirst(pageObject.deleteCommLocator)
                .first()
                .click();

            // act
            browserHelper.acceptAlert();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/communication/" });
        });

        it("should not delete comm when delete is dismissed", () => {
            // arrange
            pageObject.commRepeater
                .selectFirst(pageObject.deleteCommLocator)
                .first()
                .click();

            // act
            browserHelper.dismissAlert();

            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/communication/" });
        });

        it("should save when target looses focus", () => {
            // arrange

            // act
            pageObject.commRepeater
                .selectFirst(pageObject.targetLocator)
                .sendKeys("NewTarget", protractor.Key.TAB);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/communication/" });
        });

        it("should save when purpose looses focus", () => {
            // arrange

            // act
            pageObject.commRepeater
                .selectFirst(pageObject.purposeLocator)
                .sendKeys("NewPurpose", protractor.Key.TAB);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/communication/" });
        });

        it("should save when message looses focus", () => {
            // arrange

            // act
            pageObject.commRepeater
                .selectFirst(pageObject.messageLocator)
                .sendKeys("NewMessage", protractor.Key.TAB);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/communication/" });
        });

        it("should save when media looses focus", () => {
            // arrange

            // act
            pageObject.commRepeater
                .selectFirst(pageObject.mediaLocator)
                .sendKeys("NewMedia", protractor.Key.TAB);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/communication/" });
        });

        it("should save when due date looses focus", () => {
            // arrange

            // act
            pageObject.commRepeater
                .selectFirst(pageObject.dueDateLocator)
                .sendKeys(protractor.Key.CONTROL, "a", protractor.Key.NULL, "01-01-2017", protractor.Key.TAB);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/communication/" });
        });

        it("should save when responsible user changes", () => {
            // arrange
            var responsibleSelect = pageObject.getResponsibleSelect(0);

            // act
            responsibleSelect.then(selector => selector.selectFirst("2"));

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/communication/" });
        });
    });
});
