import mock = require("protractor-http-mock");
import Helper = require("../../helper");
import ItInterfaceEditPo = require("../../../app/components/it-system/it-interface/it-interface-edit.po");

describe("system interface edit", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: ItInterfaceEditPo;
    var mockDependencies: Array<string> = [
        "itInterface",
        "tsa",
        "interface",
        "interfaceType",
        "method",
        "datatype",
        "datarow",
        "organization"
    ];

    beforeEach(() => {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);

        pageObject = new ItInterfaceEditPo();

        browser.driver.manage().window().maximize();
    });

    afterEach(() => {
        mock.teardown();
    });

    describe("with no write access", () => {
        beforeEach(done => {
            mock(["itInterfaceNoWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should disable inputs", () => {
            // arrange

            // act

            // assert
            expect(pageObject.nameElement).toBeDisabled();
            expect(pageObject.idElement).toBeDisabled();
            expect(pageObject.versionElement).toBeDisabled();
            expect(pageObject.belongsToSelect.element).toBeSelect2Disabled();
            expect(pageObject.accessModifierSelect.element).toBeSelect2Disabled();
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itInterfaceWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should not delete when delete confirm popup is dismissed", () => {
            // arrange
            pageObject.deleteInterfaceElement.first().click();

            // act
            browserHelper.dismissAlert();

            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itinterface/1" });
        });

        it("should delete when delete confirm popup is accepted", () => {
            // arrange
            pageObject.deleteInterfaceElement.first().click();

            // act
            browserHelper.acceptAlert();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itinterface/1" });
        });

        it("should save when name looses focus", () => {
            // arrange

            // act
            pageObject.nameInput(`SomeName${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });

        it("should save when id looses focus", () => {
            // arrange

            // act
            pageObject.idInput(`SomeId${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });

        it("should save when version looses focus", () => {
            // arrange

            // act
            pageObject.versionInput(`SomeVersion${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });

        it("should save when belongs to is changed", () => {
            // arrange

            // act
            pageObject.belongsToSelect.selectFirst("i");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });

        it("should save when access modifier is changed", () => {
            // arrange

            // act
            pageObject.accessModifierSelect.selectFirst("p");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });

        it("should save when description looses focus", () => {
            // arrange

            // act
            pageObject.descriptionInput(`SomeDescription${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });

        it("should save when url looses focus", () => {
            // arrange

            // act
            pageObject.urlInput(`SomeUrl${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });

        it("should save when note looses focus", () => {
            // arrange

            // act
            pageObject.versionInput(`SomeNote${protractor.Key.TAB}`);

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });
    });
});
