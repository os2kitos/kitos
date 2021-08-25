import Login = require("../../Helpers/LoginHelper");
import ItContractOverview = require("../../PageObjects/it-contract/ItContractOverview.po");
import SystemOverview = require("../../PageObjects/it-system/Catalog/ItSystemCatalog.po");
import ItProjectOverview = require("../../PageObjects/It-project/ItProjectOverview.po");
import ItSystemInterfaceCatalog = require("../../PageObjects/it-system/Interfaces/itSystemInterface.po");
import ReportOverview = require("../../PageObjects/report/ReportOverview.po");
import SystemUsageHelper = require("../../Helpers/SystemUsageHelper");
import Constants = require("../../Utility/Constants");
import ItSystemUsageCommon = require("../../PageObjects/it-system/Usage/Tabs/ItSystemUsageCommon.po");
import UsersPage = require("../../PageObjects/Organization/UsersPage.po");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");

var loginHelper = new Login();
var itContractPage = new ItContractOverview();
var systemPage = new SystemOverview();
var systemUsagePage = new ItSystemUsageCommon();
var projectPage = new ItProjectOverview();
var interfacePage = new ItSystemInterfaceCatalog();
var reportPage = new ReportOverview();
var usersPage = new UsersPage();
var consts = new Constants();
var testFixture = new TestFixtureWrapper();

describe("For user without additional roles", () => {

    beforeAll(() => {
        testFixture.enableLongRunningTest();
        loginHelper.loginAsRegularUser();
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });

    it("Create IT contract is disabled", () => {
        checkContractAccessibility(false, true);
    });

    it("Create IT system is disabled", () => {
        checkSystemAccessibility(false, true);
    });

    it("Delete System Usage not visible", () => {
        checkSystemUsageAccessibility(false, false);
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
        checkUsersAccessibility(false, true);
    });
});

describe("For Local Administrator", () => {

    beforeAll(() => {
        loginHelper.loginAsLocalAdmin();
    });

    afterAll(() => {
        testFixture.cleanupState();
    });

    it("Create IT contract is enabled", () => {
        checkContractAccessibility(true, true);
    });

    it("Create IT system is disabled", () => {
        checkSystemAccessibility(false, true);
    });

    it("Delete System Usage is enabled", () => {
        checkSystemUsageAccessibility(true, true);
    });

    it("Create IT interface is disabled", () => {
        checkInterfaceAccessibility(false, true);
    });

    it("Create IT project is enabled", () => {
        checkProjectAccessibility(true, true);
    });

    it("Create IT report is disabled", () => {
        checkReportAccessibility(false, true);
    });

    it("Create User is enabled", () => {
        checkUsersAccessibility(true, true);
    });
});

describe("For Global Administrator", () => {

    beforeAll(() => {
        loginHelper.loginAsGlobalAdmin();
    });

    afterAll(() => {
        testFixture.cleanupState();
    });

    it("Create IT contract is enabled", () => {
        checkContractAccessibility(true, true);
    });

    it("Create IT system is enabled", () => {
        checkSystemAccessibility(true, true);
    });

    it("Delete System Usage is enabled", () => {
        checkSystemUsageAccessibility(true, true);
    });

    it("Create IT interface is enabled", () => {
        checkInterfaceAccessibility(true, true);
    });

    it("Create IT project is enabled", () => {
        checkProjectAccessibility(true, true);
    });

    it("Create IT report is enabled", () => {
        checkReportAccessibility(true, true);
    });

    it("Create User is enabled", () => {
        checkUsersAccessibility(true, true);
    });
});

function logExpectations(enabled: boolean, visible: boolean) {
    console.log(`Expecting: enabled: ${enabled}, visible: ${visible}`);
}

function checkSystemUsageAccessibility(enabled: boolean, present: boolean) {

    var promise = SystemUsageHelper.openLocalSystem(consts.defaultSystemUsageName);

    if (present === false) {
        //Not present - check if it is not there
        return promise.then(() => expect(systemUsagePage.getDeleteButtons().count()).toBe(0));;
    }

    //If expected check the state
    return promise
        .then(() => expect(systemUsagePage.getDeleteButton().isDisplayed()).toBe(true))
        .then(() => expect(systemUsagePage.getDeleteButton().isEnabled()).toBe(enabled));
}

function checkUsersAccessibility(enabled: boolean, visible: boolean) {
    return performAccessibilityCheck(() => usersPage.getPage(), () => usersPage.getCreateUserButton(), visible, enabled);
}

function checkReportAccessibility(enabled: boolean, visible: boolean) {
    return performAccessibilityCheck(() => reportPage.getPage(), () => reportPage.getCreateReportButton(), visible, enabled);
}

function checkInterfaceAccessibility(enabled: boolean, visible: boolean) {
    return performAccessibilityCheck(() => interfacePage.getPage(), () => interfacePage.getCreateInterfaceButton(), visible, enabled);
}

function checkProjectAccessibility(enabled: boolean, visible: boolean) {
    return performAccessibilityCheck(() => projectPage.getPage(), () => projectPage.getCreateProjectButton(), visible, enabled);
}

function checkSystemAccessibility(enabled: boolean, visible: boolean) {
    return performAccessibilityCheck(() => systemPage.getPage(), () => systemPage.getCreateSystemButton(), visible, enabled);
}

function checkContractAccessibility(enabled: boolean, visible: boolean) {
    return performAccessibilityCheck(() => itContractPage.getPage(), () => itContractPage.getCreateContractButton(), visible, enabled);
}
function performAccessibilityCheck(
    loadFunc,
    getButtonFunc,
    isVisible: boolean,
    isEnabled: boolean) {

    logExpectations(isEnabled, isVisible);
    return loadFunc()
        .then(() => expect(getButtonFunc().isDisplayed()).toBe(isVisible))
        .then(() => expect(getButtonFunc().getAttribute('disabled').valueOf()).toBe(isEnabled ? null : 'true')); //isEnabled does not work for sub nav anchors so we use a more precise assertion
}