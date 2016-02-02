import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import PageObject = require("../../../../app/components/it-system/edit/tabs/it-system-edit-tab-kle.po");

describe("system edit tab kle", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: PageObject;
    var mockDependencies: Array<string> = [
        "itSystem",
        "itSystemKle",
        "businessType",
        "itSystemTypeOption",
        "itInterfaceUse",
        "organization",
        "itInterface",
        "taskref"
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
            mock(["itSystemNoWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should disable all checkboxes", () => {
            // arrange

            // act

            // assert
            pageObject.taskRepeater.each(elem => {
                var checkbox = elem.element(pageObject.checkboxLocator);

                expect(checkbox).toBeDisabled();
            });
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            // TODO: itsystem/1/taskId=* is hardcoded in mock JSON for IDs 19-68. Refactor to object based mock generation.
            mock(["itSystemWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should save when task is checked", () => {
            // arrange

            // act
            pageObject.taskRepeater.selectFirst(pageObject.checkboxLocator).first().click()
                .then(() => {

                    // assert
                    expect(mockHelper.lastRequest()).toMatchRequest({ method: "POST", url: "api/itSystem/1" });
                });
        });

        it("should repeat tasks", () => {
            // arrange

            // act

            // assert
            expect(pageObject.taskRepeater.count()).toBeGreaterThan(0);
        });

        it("should disable group selector when main group is not selected", () => {
            // arrange

            // act
            // catch error to ignore nothing selected error
            pageObject.mainGroupSelect.deselect().thenCatch(() => null);

            // assert
            expect(pageObject.groupSelect.element).toBeSelect2Disabled();
        });

        it("should enable group selector when main group is selected", () => {
            // arrange

            // act
            pageObject.mainGroupSelect.selectFirst();

            // assert
            expect(pageObject.groupSelect.element).not.toBeSelect2Disabled();
        });

        it("should get tasks when main group is selected", () => {
            // arrange

            // act
            pageObject.mainGroupSelect.selectFirst();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "GET", url: "api/itSystem/1?(.)*taskGroup=[0-9]" });
        });

        it("should get tasks when group is selected", () => {
            // arrange
            pageObject.mainGroupSelect.selectFirst();
            mock.clearRequests();

            // act
            pageObject.groupSelect.selectFirst();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "GET", url: "api/itSystem/1?(.)*taskGroup=[0-9]" });
        });

        it("should get selected tasks only on show selected click", () => {
            // arrange

            // act
            pageObject.changeTaskViewElement.click();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "GET", url: "api/itSystem/1?(.)*tasks=true(.)*onlySelected=true" });
        });

        it("should check all tasks on confirmed select all pages click", () => {
            // arrange
            pageObject.selectAllPagesElement.click();

            // act
            browserHelper.acceptAlert();

            // assert
            // match api url with query string 'taskID=' only and ignoring any other query parameters
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/itSystem/1?(.[^ ])*taskId=[^0-9]" });
        });

        it("should not check all tasks on dismissed select all pages click", () => {
            // arrange
            pageObject.selectAllPagesElement.click();

            // act
            browserHelper.dismissAlert();

            // assert
            // match api url with query string 'taskID=' only and ignoring any other query parameters
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "POST", url: "api/itSystem/1?(.[^ ])*taskId=[^0-9]" });
        });

        it("should check all tasks on page on confirmed select all click", () => {
            // arrange
            pageObject.selectAllElement.click();

            // act
            browserHelper.acceptAlert();

            // assert
            pageObject.taskRepeater.each(elem => {
                var checkbox = elem.element(pageObject.checkboxLocator);

                // match api url with query string 'taskID=ID' only and ignoring any other query parameters
                checkbox.getAttribute("id")
                    .then(id =>
                        expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: `api/itSystem/1?(.[^ ])*taskId=${id}` }));

                expect(checkbox).toBeChecked();
            });
        });

        it("should not check all tasks on page on dismissed select all click", () => {
            // arrange
            pageObject.selectAllElement.click();

            // act
            browserHelper.dismissAlert();

            // assert
            pageObject.taskRepeater.each(elem => {
                var checkbox = elem.element(pageObject.checkboxLocator);

                // match api url with query string 'taskID=ID' only and ignoring any other query parameters
                checkbox.getAttribute("id")
                    .then(id =>
                        expect(mock.requestsMade()).not.toMatchInRequests({ method: "POST", url: `api/itSystem/1?(.[^ ])*taskId=${id}` }));

                expect(checkbox).not.toBeChecked();
            });
        });

        it("should uncheck all tasks on page on confirmed deselect all click", () => {
            // arrange
            pageObject.selectAllElement.click();
            browserHelper.acceptAlert();
            mock.clearRequests();
            pageObject.deselectAllElement.click();

            // act
            browserHelper.acceptAlert();

            // assert
            pageObject.taskRepeater.each(elem => {
                var checkbox = elem.element(pageObject.checkboxLocator);

                // match api url with query string 'taskID=ID' only and ignoring any other query parameters
                checkbox.getAttribute("id")
                    .then(id =>
                        expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: `api/itSystem/1?(.[^ ])*taskId=${id}` }));

                expect(checkbox).not.toBeChecked();
            });
        });

        it("should not uncheck all tasks on page on dismissed deselect all click", () => {
            // arrange
            pageObject.selectAllElement.click();
            browserHelper.acceptAlert();
            mock.clearRequests();
            pageObject.deselectAllElement.click();

            // act
            browserHelper.dismissAlert();

            // assert
            pageObject.taskRepeater.each(elem => {
                var checkbox = elem.element(pageObject.checkboxLocator);

                // match api url with query string 'taskID=ID' only and ignoring any other query parameters
                checkbox.getAttribute("id")
                    .then(id =>
                        expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: `api/itSystem/1?(.[^ ])*taskId=${id}` }));

                expect(checkbox).toBeChecked();
            });
        });

        it("should uncheck all tasks on confirmed deselect all pages click", () => {
            // arrange
            pageObject.selectAllElement.click();
            browserHelper.acceptAlert();
            mock.clearRequests();
            pageObject.deselectAllPages.click();

            // act
            browserHelper.acceptAlert();

            // assert
            // match api url with query string 'taskID=' only and ignoring any other query parameters
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itSystem/1?(.[^ ])*taskId=[^0-9]" });
        });

        it("should not uncheck all tasks on dismissed deselect all pages click", () => {
            // arrange
            pageObject.selectAllElement.click();
            browserHelper.acceptAlert();
            mock.clearRequests();
            pageObject.deselectAllPages.click();

            // act
            browserHelper.dismissAlert();

            // assert
            // match api url with query string 'taskID=' only and ignoring any other query parameters
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itSystem/1?(.[^ ])*taskId=[^0-9]" });
        });
    });
});
