import Login = require("../Helpers/LoginHelper");
import DPA = require("../PageObjects/Local-admin/LocalDataProcessorAgreement.po");


describe("Local admin is able to toggle DataProcessorAgreement", () => {

    var loginHelper = new Login();
    var DPAPageHelper = new DPA();

    beforeAll(() => {
        loginHelper.loginAsGlobalAdmin();
    });

    it("Option to toggle DataProcessorAgreement is visible", () => {
        DPAPageHelper.getPage();
    });

});

