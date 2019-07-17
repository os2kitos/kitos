import Login = require("../../Helpers/LoginHelper");
import CatalogHelper = require("../../Helpers/CatalogHelper");
import ItSystemEditPo = require("../../PageObjects/it-system/Catalog/ItSystemCatalog.po")
import Constants = require("../../Utility/Constants");

describe("Regular user tests", () => {
    var loginHelper = new Login();
    var pageObject = new ItSystemEditPo();
    var consts = new Constants();

    beforeEach(() => {
        pageObject.getPage();
        loginHelper.loginAsRegularUser();
        browser.waitForAngular();
    });

    it("Regular user can create a catalog", () => {
        CatalogHelper.createCatalog(consts.defaultCatalog);
    });

});




