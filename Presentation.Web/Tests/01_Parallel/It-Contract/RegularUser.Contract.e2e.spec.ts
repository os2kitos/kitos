import Login = require("../../Helpers/LoginHelper");
import ItContractOverview = require("../../PageObjects/it-contract/ItContractOverview.po");
import Constants = require("../../Utility/Constants");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");

describe("Regular user has access to features in the contract overview", () => {

    var loginHelper = new Login();
    var consts = new Constants();
    var pageObject = new ItContractOverview();
    var columnObject = pageObject.kendoToolbarWrapper.columnObjects();
    var testFixture = new TestFixtureWrapper();

    beforeAll(() => {
        testFixture.enableLongRunningTest();
        loginHelper.loginAsRegularUser();
    });

    beforeEach(() => {
        pageObject.getPage()
            .then(() => pageObject.waitForKendoGrid());
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });

    it("User can see contract ", () => {
        expect(columnObject.contractName.getText()).toContain(consts.defaultItContractName);
    });
});