import mock = require("protractor-http-mock");
import Helper = require("../../helper");
import ItInterfaceEditTabDetailsPo = require("../../../app/components/it-system/it-interface/it-interface-edit-tab-details.po");

describe("system interface edit tab details", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: ItInterfaceEditTabDetailsPo;
    var mockDependencies: Array<string> = [
        "itInterface",
        "tsa",
        "interface",
        "interfaceType",
        "method",
        "datatype",
        "datarow",
        "organization",
        "itSystem",
        "exhibit"
    ];

    beforeEach(() => {
        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);

        pageObject = new ItInterfaceEditTabDetailsPo();

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
            expect(pageObject.tsaSelector.element).toBeSelect2Disabled();
            expect(pageObject.typeSelector.element).toBeSelect2Disabled();
            expect(pageObject.interfaceSelector.element).toBeSelect2Disabled();
            expect(pageObject.methodSelector.element).toBeSelect2Disabled();
            expect(pageObject.exhibitSelector.element).toBeSelect2Disabled();

            pageObject.dataRowRepeater.each(el => {
                expect(el.element(pageObject.dataLocator)).toBeDisabled();
                expect(pageObject.dataTypeSelector.element).toBeSelect2Disabled();
            });
        });

        it("should hide delete and add data row buttons", () => {
            // arrange

            // act

            // assert
            pageObject.dataRowRepeater.each(el => {
                expect(el.element(pageObject.deleteLocator)).not.toBeVisible();
            });
            expect(pageObject.addDataButton).not.toBeVisible();
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itInterfaceWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should save when tsa to is changed", () => {
            // arrange

            // act
            pageObject.tsaSelector.selectFirst("n");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });

        it("should save when type is changed", () => {
            // arrange

            // act
            pageObject.typeSelector.selectFirst("p");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });

        it("should save when interface is changed", () => {
            // arrange

            // act
            pageObject.interfaceSelector.selectFirst("l");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });

        it("should save when method is changed", () => {
            // arrange

            // act
            pageObject.methodSelector.selectFirst("a");

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itInterface/1" });
        });

        it("should save when exhibit is changed", () => {
            // arrange

            // act
            pageObject.exhibitSelector.selectFirst("i");

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/exhibit" });
        });

        it("should add new data row when add is clicked", () => {
            // arrange

            // act
            pageObject.addDataButton.click();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/dataRow" });
        });

        it("should delete data row when delete is clicked", () => {
            // arrange

            // act
            pageObject.dataRowRepeater
                .selectFirst(pageObject.deleteLocator)
                .first()
                .click();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/dataRow/1" });
        });

        it("should save when data looses focus", () => {
            // arrange

            // act
            pageObject.dataRowRepeater
                .selectFirst(pageObject.dataLocator)
                .first()
                .sendKeys(`SomeDate${protractor.Key.TAB}`);

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "PATCH", url: "api/dataRow/1" });
        });

        it("should save when data type is changed", () => {
            // arrange

            // act
            pageObject.dataTypeSelector.selectFirst();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "PATCH", url: "api/dataRow/1" });
        });
    });
});
