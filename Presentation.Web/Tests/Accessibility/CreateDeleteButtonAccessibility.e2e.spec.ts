import Login = require("../Helpers/LoginHelper");
import ItContractOverview = require("../PageObjects/it-contract/ItContractOverview.po");
import SystemOverview = require("../PageObjects/it-system/Catalog/ItSystemCatalog.po");
import ItProjectOverview = require("../PageObjects/It-project/ItProjectOverview.po");
import ItSystemInterfaceCatalog = require("../PageObjects/it-system/Interfaces/itSystemInterface.po");
import ReportOverview = require("../PageObjects/report/ReportOverview.po");

var loginHelper = new Login();
var itContractPage = new ItContractOverview();
var systemPage = new SystemOverview();
var projectPage = new ItProjectOverview();
var interfacePage = new ItSystemInterfaceCatalog();
var reportPage = new ReportOverview();

describe("For user without additional roles", () => {

    beforeAll(() => {
        loginHelper.loginAsRegularUser();
    });

    it("Create IT contract is disabled", () => {
        checkContractAccessibility(false, true);
    });

    it("Create IT system is disabled", () => {
        checkSystemAccessibility(false, true);
    });

    it("Delete System Usage not available", () => {
        //TODO
    });

    it("Create IT interface is disabled", () => {
        checkInterfaceAccessibility(false, true);
    });

    it("Create IT project is disabled", () => {
        checkProjectAccessibility(false, true);
    });

    it("Create IT report is disabled", () => {
        checkReportAccessibility(false, true);
    });

    it("Create User is disabled", () => {
        //TODO
    });
});

function logExpectations(enabled: boolean, visible: boolean) {
    console.log(`Expecting: enabled: ${enabled}, visible: ${visible}`);
}


function checkReportAccessibility(enabled: boolean, visible: boolean) {
    logExpectations(enabled, visible);
    return reportPage.getPage()
        .then(() => reportPage.waitForKendoGrid())
        .then(() => expect(reportPage.getCreateReportButton().isEnabled()).toBe(enabled))
        .then(() => expect(reportPage.getCreateReportButton().isDisplayed()).toBe(visible));
}
function checkInterfaceAccessibility(enabled: boolean, visible: boolean) {
    logExpectations(enabled, visible);
    return interfacePage.getPage()
        .then(() => interfacePage.waitForKendoGrid())
        .then(() => expect(interfacePage.getCreateInterfaceButton().isEnabled()).toBe(enabled))
        .then(() => expect(interfacePage.getCreateInterfaceButton().isDisplayed()).toBe(visible));
}

function checkProjectAccessibility(enabled: boolean, visible: boolean) {
    logExpectations(enabled, visible);
    return projectPage.getPage()
        .then(() => projectPage.waitForKendoGrid())
        .then(() => expect(projectPage.getCreateProjectButton().isEnabled()).toBe(enabled))
        .then(() => expect(projectPage.getCreateProjectButton().isDisplayed()).toBe(visible));
}

function checkSystemAccessibility(enabled: boolean, visible: boolean) {
    logExpectations(enabled, visible);
    return systemPage.getPage()
        .then(() => systemPage.waitForKendoGrid())
        .then(() => expect(systemPage.getCreateSystemButton().isEnabled()).toBe(enabled))
        .then(() => expect(systemPage.getCreateSystemButton().isDisplayed()).toBe(visible));
}

function checkContractAccessibility(enabled: boolean, visible: boolean) {
    logExpectations(enabled, visible);
    return itContractPage.getPage()
        .then(() => itContractPage.waitForKendoGrid())
        .then(() => expect(itContractPage.getCreateContractButton().isEnabled()).toBe(enabled))
        .then(() => expect(itContractPage.getCreateContractButton().isDisplayed()).toBe(visible));
}