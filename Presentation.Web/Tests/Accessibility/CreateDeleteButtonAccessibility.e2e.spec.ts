import Login = require("../Helpers/LoginHelper");
import ItContractOverview = require("../PageObjects/it-contract/ItContractOverview.po");
import SystemOverview = require("../PageObjects/it-system/Catalog/ItSystemCatalog.po");

var loginHelper = new Login();
var itContractPage = new ItContractOverview();
var systemPage = new SystemOverview();

describe("Regular user cannot see or use buttons", () => {

    beforeAll(() => {
        loginHelper.loginAsRegularUser();
    });

    it("Create IT contract is disabled", () => {
        expectClickabilityOfCreateContractButtonToBe(false);
        expectVisibilityOfCreateContractButtonToBe(true);
    });

    it("Create IT system is disabled", () => {
        expectClickabilityOfCreateSystemButtonToBe(false);
        expectVisibilityOfCreateSystemButtonToBe(true);
    });

});


function expectVisibilityOfCreateSystemButtonToBe(visibility: boolean) {
    systemPage.getPage()
        .then(() => systemPage.waitForKendoGrid())
        .then(() => expect(systemPage.getCreateSystemButton().isDisplayed()).toBe(visibility));
}

function expectClickabilityOfCreateSystemButtonToBe(isEnabled: boolean) {
    systemPage.getPage()
        .then(() => systemPage.waitForKendoGrid())
        .then(() => expect(systemPage.getCreateSystemButton().isEnabled()).toBe(isEnabled));
}

function expectVisibilityOfCreateContractButtonToBe(visibility: boolean) {
    itContractPage.getPage()
        .then(() => itContractPage.waitForKendoGrid())
        .then(() => expect(itContractPage.getCreateContractButton().isDisplayed()).toBe(visibility));
}

function expectClickabilityOfCreateContractButtonToBe(isEnabled: boolean) {
    itContractPage.getPage()
        .then(() => itContractPage.waitForKendoGrid())
        .then(() => expect(itContractPage.getCreateContractButton().isEnabled()).toBe(isEnabled));
}