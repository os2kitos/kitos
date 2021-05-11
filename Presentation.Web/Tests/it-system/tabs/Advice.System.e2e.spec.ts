
import login = require("../../Helpers/LoginHelper");
import ItSystemReferenceHelper = require("../../PageObjects/it-system/tabs/ItSystemReference.po");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import ItSystemHelper = require("../../Helpers/SystemCatalogHelper");
import AdviceHelper = require("../../Helpers/AdviceHelper");

describe("Is able to create and edit advice",
    () => {
        var loginHelper = new login();
        var itSystemReference = new ItSystemReferenceHelper();
        var testFixture = new TestFixtureWrapper();
        var itSystemName = createItSystemName();
        var adviceHelper = new AdviceHelper();

        beforeAll(() => {
            loginHelper.loginAsGlobalAdmin()
                .then(() => ItSystemHelper.createSystem(itSystemName));
        });

        beforeEach(() => {
            testFixture.enableLongRunningTest();
        });

        afterAll(() => {
            testFixture.disableLongRunningTest();
            testFixture.cleanupState();
        });

        it("Is able to create a new advice",
            () => {

                adviceHelper.goToSpecificItSystemAdvice(itSystemName)
                    .then(() => adviceHelper.createNewAdvice("mbk@strongminds.dk", "mbk@strongminds.dk", "01-05-2021","31-05-2021"));
            });
    });

function createItSystemName() {
    return `ItSystemAdviceTest${new Date().getTime()}`;
}

