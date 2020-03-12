import login = require("../../../Helpers/LoginHelper");
import ItSystemCatalogHelper = require("../../../Helpers/SystemCatalogHelper");
import itSystemHelper = require("../../../Helpers/SystemUsageHelper");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import ItSystemUsageGDPR = require("../../../PageObjects/It-system/Usage/Tabs/ItSystemUsageGDPR.po");
import ItSystemMainTabPage = require("../../../PageObjects/It-system/Usage/Tabs/ItSystemUsageMain.po")
import Select2Helper = require("../../../Helpers/Select2Helper");

describe("User is able to", () => {

    var loginHelper = new login();
    var testFixture = new TestFixtureWrapper();
    var itSystem1 = createItSystemName();


    beforeAll(() => {
        loginHelper.loginAsGlobalAdmin();
    });

    beforeEach(() => {
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.disableLongRunningTest();
        testFixture.cleanupState();
    });

    it("User is able to select a Personal Data type",
        () => {
            ItSystemCatalogHelper.createSystem(itSystem1)
                .then(() => ItSystemCatalogHelper.getActivationToggleButton(itSystem1).click())
                .then(() => itSystemHelper.openLocalSystem(itSystem1))
                .then(() => console.log("Entering data into field"))
                .then(() => Select2Helper.selectMultiChoiceValue("Almindelig","s2id_sensitive-data"))
                .then(() => browser.sleep(1000));

        });

    function createItSystemName() {
        return `SystemUsageMain${new Date().getTime()}`;
    }

});
