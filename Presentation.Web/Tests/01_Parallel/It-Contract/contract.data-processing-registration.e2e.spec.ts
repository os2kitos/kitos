import Login = require("../../Helpers/LoginHelper");
import ContractDprPage = require("../../PageObjects/It-contract/Tabs/ContractDpr.po");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import ContractHelper = require("../../Helpers/ContractHelper");
import DataProcessingRegistrationHelper = require("../../Helpers/DataProcessingRegistrationHelper");
import DataProcessingRegistrationEditContractPageObject = require("../../PageObjects/Data-Processing/Tabs/data-processing-registration.edit.contract.po")

describe("It-Contract data processing registration test", () => {

    const loginHelper = new Login();
    const testFixture = new TestFixtureWrapper();
    const dpaHelper = DataProcessingRegistrationHelper;
    const contractHelper = ContractHelper;
    const contractDprPage = new ContractDprPage();

    const createName = (prefix: string) => {
        return `${prefix}_${new Date().getTime()}`;
    }
    
    const dprName = createName("DPR");

    beforeAll(() => {
        testFixture.enableLongRunningTest();
        loginHelper.loginAsLocalAdmin()
            .then(() => dpaHelper.checkAndEnableDpaModule())
            .then(() => dpaHelper.createDataProcessingRegistration(dprName));
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });

    it("Assigning and removing dpr",
        () => {
            const contractName = createName("Contract");

            contractHelper.createContract(contractName)
                .then(() => contractHelper.openContract(contractName))
                .then(() => contractHelper.goToDpr())
                .then(() => contractHelper.assignDpr(dprName))
                .then(() => verifyContractContent([dprName], []))
                .then(() => verifyContractIsPresentOnDprContractPage(dprName, contractName))
                .then(() => contractHelper.removeDpr(dprName))
                .then(() => verifyContractContent([], [dprName]));
        });

    function verifyContractContent(presentDPRs: string[], unpresentDPR: string[]) {
        presentDPRs.forEach(name => {
            console.log(`Expecting dpr to be present:${name}`);
            expect(contractDprPage.getDprRow(name).isPresent()).toBeTruthy();
        });
        unpresentDPR.forEach(name => {
            console.log(`Expecting dpr NOT to be present:${name}`);
            expect(contractDprPage.getDprRow(name).isPresent()).toBeFalsy();
        });
    }

    function verifyContractIsPresentOnDprContractPage(drpName: string, contractName: string) {
        return contractHelper.clickDpr(drpName)
            .then(() => dpaHelper.goToItContracts())
            .then(() => expect(DataProcessingRegistrationEditContractPageObject.getContractLink(contractName).isPresent()).toBeTruthy())
            .then(() => dpaHelper.clickContract(contractName))
            .then(() => browser.waitForAngular())
            .then(() => contractHelper.goToDpr());
    }
});