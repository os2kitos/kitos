import Login = require("../Helpers/LoginHelper");
import ItContractOverview = require("../PageObjects/it-contract/ItContractOverview.po");
import SystemOverview = require("../PageObjects/it-system/Catalog/ItSystemCatalog.po");
import ItProjectOverview = require("../PageObjects/It-project/ItProjectOverview.po");

var loginHelper = new Login();
var itContractPage = new ItContractOverview();
var systemPage = new SystemOverview();
var projectPage = new ItProjectOverview();

describe("For user without additional roles", () => {

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

    it("Delete System Usage not available", () => {
        //TODO
    });

    it("Create IT interface is disabled", () => {
        //TODO
    });

    it("Create IT project is disabled", () => {
        expectClickabilityOfCreateProjectButtonToBe(false);
        expectVisibilityOfCreateProjectButtonToBe(true);
    });

    it("Create IT report is disabled", () => {
        //TODO
    });

    it("Create User is disabled", () => {
        //TODO
    });
});

function expectClickabilityOfCreateProjectButtonToBe(isEnabled: boolean) {
    projectPage.getPage()
        .then(() => projectPage.waitForKendoGrid())
        .then(() => expect(projectPage.getCreateProjectButton().isEnabled()).toBe(isEnabled));
}

function expectVisibilityOfCreateProjectButtonToBe(isVisible: boolean) {
    projectPage.getPage()
        .then(() => projectPage.waitForKendoGrid())
        .then(() => expect(projectPage.getCreateProjectButton().isDisplayed()).toBe(isVisible));
}

function expectVisibilityOfCreateSystemButtonToBe(visibility: boolean) {
    return systemPage.getPage()
        .then(() => systemPage.waitForKendoGrid())
        .then(() => expect(systemPage.getCreateSystemButton().isDisplayed()).toBe(visibility));
}

function expectClickabilityOfCreateSystemButtonToBe(isEnabled: boolean) {
    return systemPage.getPage()
        .then(() => systemPage.waitForKendoGrid())
        .then(() => expect(systemPage.getCreateSystemButton().isEnabled()).toBe(isEnabled));
}

function expectVisibilityOfCreateContractButtonToBe(visibility: boolean) {
    return itContractPage.getPage()
        .then(() => itContractPage.waitForKendoGrid())
        .then(() => expect(itContractPage.getCreateContractButton().isDisplayed()).toBe(visibility));
}

function expectClickabilityOfCreateContractButtonToBe(isEnabled: boolean) {
    return itContractPage.getPage()
        .then(() => itContractPage.waitForKendoGrid())
        .then(() => expect(itContractPage.getCreateContractButton().isEnabled()).toBe(isEnabled));
}