import mock = require("protractor-http-mock");
import Helper = require("../../helper");
import ItSystemEditPo = require("../../../app/components/it-system/edit/it-system-edit.po");

describe("system edit view", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: ItSystemEditPo;
    var mockDependencies: Array<string> = [
        "itSystem",
        "businessType",
        "itSystemTypeOption",
        "itInterfaceUse",
        "organization"
    ];

    beforeEach(() => {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);

        pageObject = new ItSystemEditPo();

        browser.driver.manage().window().maximize();
    });

    afterEach(() => {
        mock.teardown();
    });

    describe("with no write access", () => {
        beforeEach(done => {
            mock(["itSystemNoWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should disable inputs", () => {
            // arrange

            // act

            // assert
            expect(pageObject.appTypeSelect.element).toBeSelect2Disabled();
            expect(pageObject.nameElement).toBeDisabled();
            expect(pageObject.systemParentSelect.element).toBeSelect2Disabled();
            expect(pageObject.belongsToSelect.element).toBeSelect2Disabled();
            expect(pageObject.accessModifierSelect.element).toBeSelect2Disabled();
            expect(pageObject.usageTypeSelector.element).toBeSelect2Disabled();
            expect(pageObject.descriptionElement).toBeDisabled();
            expect(pageObject.furtherDescriptionElement).toBeDisabled();
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itSystemWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should not delete when delete confirm popup is dismissed", () => {
            // arrange
            pageObject.deleteSystemElement.click();

            // act
            browserHelper.dismissAlert();

            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itsystem/1" });
        });

        it("should delete when delete confirm popup is accepted", () => {
            // arrange
            pageObject.deleteSystemElement.click();

            // act
            browserHelper.acceptAlert();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itsystem/1" });
        });

        it("should save when appType changes", () => {
            // arrange

            // act
            pageObject.appTypeSelect.selectFirst("i");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        });

        it("should save when name looses focus", () => {
            // arrange

            // act
            pageObject.nameInput("SomeName");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        });

        it("should save when system parent is changed", () => {
            // arrange

            // act
            pageObject.systemParentSelect.selectFirst("i");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        });

        it("should save when belongs to is changed", () => {
            // arrange

            // act
            pageObject.belongsToSelect.selectFirst("i");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        });

        it("should save when access modifier is changed", () => {
            // arrange

            // act
            pageObject.accessModifierSelect.selectFirst("p");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        });

        it("should save when usage type is changed", () => {
            // arrange

            // act
            pageObject.usageTypeSelector.selectFirst("p");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        });

        it("should save when description looses focus", () => {
            // arrange

            // act
            pageObject.descriptionInput("SomeDescription");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        });

        it("should save when further description looses focus", () => {
            // arrange

            // act
            pageObject.furtherDescriptionInput("SomeFurtherDescription");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itsystem/1" });
        });
    });
});
