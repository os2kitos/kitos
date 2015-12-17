import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-status-project.po");

describe("project edit view", () => {
    var browserHelper: Helper.Browser;
    var mockHelper: Helper.Mock;
    var pageObject: PageObject;

    beforeEach(() => {
        mock(["itproject", "itprojectrole", "itprojecttype", "itprojectrights"]);

        browserHelper = new Helper.Browser(browser);
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
});
