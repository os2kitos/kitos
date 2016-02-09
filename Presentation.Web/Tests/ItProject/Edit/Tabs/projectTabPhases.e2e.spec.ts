import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-phases.po");

describe("project edit tab phases", () => {
    var mockHelper: Helper.Mock;
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

        it("should hide edit phase names option", () => {
            // arrange

            // act

            // assert
            expect(pageObject.editPhaseNamesElement).not.toBeVisible();
        });

        it("should disable phase buttons", () => {
            // arrange
            var buttons = pageObject.buttonRepeater.select(1, pageObject.buttonLocator);

            // act

            // assert
            buttons.each(element => expect(element).toBeDisabled());
        });

        it("should disable phase cross date", () => {
            // arrange
            // select index 1 as the first element in the repeater is hidden
            var elements = pageObject.crossDateRepeater.select(1, pageObject.crossDateLocator);

            // act

            // assert
            elements.each(element => expect(element).toBeDisabled());
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should show input fields when edit phase names is clicked", () => {
            // arrange

            // act
            pageObject.editPhaseNamesElement.click()
                .then(() => {

                    // assert
                    var count = pageObject.nameRepeater.selectFirst(pageObject.nameLocator).count();
                    expect(count).toBeGreaterThan(0);
                });
        });

        it("should hide input fields when edit phase names is clicked twice", () => {
            // arrange

            // act
            pageObject.editPhaseNamesElement.click();
            pageObject.editPhaseNamesElement.click();

            // assert
            var count = pageObject.nameRepeater.selectFirst(pageObject.nameLocator).count();
            expect(count).toBe(0);
        });

        it("should save when phase name looses focus", () => {
            // arrange
            pageObject.editPhaseNamesElement.click();
            pageObject.nameRepeater
                .selectFirst(pageObject.nameLocator)
                .first()
                .sendKeys("SomeText");

            // act
            pageObject.editPhaseNamesElement.click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "POST", url: "api/itproject/1" });
        });

        it("should save when phase button is clicked", () => {
            // arrange

            // act
            pageObject.buttonRepeater.select(1, pageObject.buttonLocator).click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itproject/1" });
        });

        it("should save when phase cross date is changed", () => {
            // arrange
            // select index 1 as the first element in the repeater is hidden
            var element = pageObject.crossDateRepeater.select(1, pageObject.crossDateLocator).first();

            // act
            element.sendKeys("1")
                .then(() => {
                    // assert
                    expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/itproject/1?(.*?)phaseNum=1" });
                    expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/itproject/1?(.*?)phaseNum=2" });
                });
        });
    });
});
