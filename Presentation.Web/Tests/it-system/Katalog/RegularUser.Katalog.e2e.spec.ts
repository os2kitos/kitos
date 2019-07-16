import login = require("../../Helpers/LoginHelper");
import KatalogHelper = require("../../Helpers/KatalogHelper");
import ItSystemEditPo = require("../../PageObjects/it-system/Katalog/ItSystemKatalog.po")
import constants = require("../../Utility/Constants");

describe("Regular user tests", () => {
    //var EC = protractor.ExpectedConditions;
    var loginHelper = new login();
    var pageObject = new ItSystemEditPo();
    var consts = new constants();
    //var headerButtons = pageObject.kendoToolbarWrapper.headerButtons();
  // var headerButtonsHelper = pageObject.kendoToolbarHelper.headerButtons;

    beforeAll(() => {
    });

    beforeEach(() => {
        pageObject.getPage();
        loginHelper.loginAsRegularUser();
        browser.waitForAngular();
    });

    it("Regular user can create a catalog", () => {
        KatalogHelper.createKatalog(consts.defaultCatalog);
    });

});




