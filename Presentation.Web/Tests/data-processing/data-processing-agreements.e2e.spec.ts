import Login = require("../Helpers/LoginHelper");
import TestFixtureWrapper = require("../Utility/TestFixtureWrapper");
import DataProcessingAgreementOverviewPageObject =
require("../PageObjects/Data-Processing/data-processing-agreement.overview.po");
import WaitTimers = require("../Utility/WaitTimers");

describe("Data processing agreement tests", () => {

    const loginHelper = new Login();
    const pageObject = new DataProcessingAgreementOverviewPageObject();
    const testFixture = new TestFixtureWrapper();
    const waitUpTo = new WaitTimers();

    const createName = (index: number) => {
        return `Dpa{new Date().getTime()}_${index}`;
    }

    beforeAll(() => {
        loginHelper.loginAsLocalAdmin();
        testFixture.enableLongRunningTest();
        //TODO: Make sure dpa is enabled  for the organization
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });

    it("It is possible to create new data processing agreements", () => {
        const name = createName(1);
        pageObject
            .getPage()
            .then(() => pageObject.waitForKendoGrid())
            .then(() => pageObject.getCreateDpaButton().click())
            .then(() => browser.wait(pageObject.isCreateDpaAvailable(), waitUpTo.twentySeconds))
            .then(() => pageObject.getNewDpaNameInput().sendKeys(name))
            .then(() => pageObject.getNewDpaSubmitButton().click())
            .then(() => pageObject.waitForKendoGrid())
            .then(() => expect(pageObject.findSpecificDpaInNameColumn(name)).toBeDefined());
    });
});