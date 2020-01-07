import Login = require("../Helpers/LoginHelper");
import Misc = require("../PageObjects/Global-admin/GlobalMisc.po");
import Constants = require("../Utility/Constants");

describe("Global Administrator is able to see changes to KLE and update to the newest KLE version", () => {

    var loginHelper = new Login();
    var consts = new Constants();
    var pageObject = new Misc();

    beforeAll(() => {
        loginHelper.loginAsGlobalAdmin();
    });

    beforeEach(() => {
        pageObject.getPage();
        browser.waitForAngular();
    });

    it("Is able to see if there is an update for KLE", () => {
        expect(element(by.id("KLEStatusLabel")).isDisplayed());
    });

    it("Is able to download xml file with changes to the newest version", () => {
       //TODO
    });

    it("Is able to import newest KLE changes", () => {
        //TODO
    });

});

