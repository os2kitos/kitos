import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-org.po");

describe("project edit tab org", () => {
    var mockHelper: Helper.Mock;
    var pageObject: PageObject;
    var mockDependencies: Array<string> = [
        "itproject",
        "itprojecttype",
        "itProjectOrgUnitUsage",
        "organizationunit"
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

        it("should disable responsible organization dropdown", () => {
            // arrange

            // act

            // assert
            expect(pageObject.responsibleOrgSelector.element).toBeSelect2Disabled();
        });

        it("should disable organization elements", () => {
            // arrange
            var elements = element.all(pageObject.orgUnitLocator);

            // act

            // assert
            elements.each((element, index) => {
                expect(element).toBeDisabled();
            });
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should save when organization is selected", () => {
            // arrange

            // act
            pageObject.orgUnitsTreeRepeater.selectFirst(pageObject.orgUnitLocator).click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "POST", url: "api/itproject/1" });
        });

        it("should save when organization is deselected", () => {
            // arrange
            pageObject.orgUnitsTreeRepeater.selectFirst(pageObject.orgUnitLocator).click();

            // act
            pageObject.orgUnitsTreeRepeater.selectFirst(pageObject.orgUnitLocator).click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "DELETE", url: "api/itproject/1" });
        });

        it("should present no units in dropdown when organization is deselected", () => {
            // arrange
            pageObject.orgUnitsTreeRepeater.selectFirst(pageObject.orgUnitLocator).click();
            pageObject.orgUnitsTreeRepeater.selectFirst(pageObject.orgUnitLocator).click();

            // act
            pageObject.responsibleOrgSelector.click();
            var selectorCount = pageObject.responsibleOrgSelector.options.count();

            // assert
            expect(selectorCount).toBe(0, "selector has options");
        });

        it("should present units in dropdown when organization is selected", () => {
            // arrange
            pageObject.orgUnitsTreeRepeater.selectFirst(pageObject.orgUnitLocator).click();

            // act
            pageObject.responsibleOrgSelector.click();
            var selectorCount = pageObject.responsibleOrgSelector.options.count();

            // assert
            expect(selectorCount).toBeGreaterThan(0, "selector hos no options");
        });
    });
});
