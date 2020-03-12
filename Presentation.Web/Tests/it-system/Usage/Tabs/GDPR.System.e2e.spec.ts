import login = require("../../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import ItSystemHelper = require("../../../Helpers/SystemCatalogHelper");
import Select2Helper = require("../../../Helpers/Select2Helper");
import TestFixtureWrapper1 = require("../../../Utility/TestFixtureWrapper");
import ItSystemReference1 = require("../../../PageObjects/It-system/Tabs/ItSystemReference.po");
import ItSystemReference = require("../../PageObjects/it-system/Tabs/ItSystemFrontpage.po");

describe("Global Admin can",
    () => {
        const optionInputs = {
            Undecided: { text: " " },
            B: { text: "B" }
        };
        var loginHelper = new login();
        var testFixture = new TestFixtureWrapper1();
        var pageObject = new ItSystemReference1();

        var itSystemName = `ItSystemMainTabTest${new Date().getTime()}`;

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


    });
