import Login = require("../Helpers/LoginHelper");
import DPA = require("../PageObjects/Local-admin/LocalDataProcessingAgreement.po");


describe("Local admin is able to toggle DataProcessingAgreement", () => {

    var loginHelper = new Login();
    var DPAPageHelper = new DPA();

    beforeAll(() => {
        loginHelper.loginAsGlobalAdmin();
    });

    it("Option to toggle DataProcessingAgreement is visible", () => {
        DPAPageHelper.getPage();
    });

});

