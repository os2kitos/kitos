import login = require("../../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import ItSystemHelper = require("../../../Helpers/SystemCatalogHelper");
import RelationHelper = require("../../../Helpers/RelationHelper");

describe("User is able to create and view relation",
    () => {
        var loginHelper = new login();
        var testFixture = new TestFixtureWrapper();

        var relationSystemName1 = createItSystemName() + "1";
        var relationSystemName2 = createItSystemName() + "2";

        beforeAll(() => {
            loginHelper.loginAsGlobalAdmin();
            ItSystemHelper.createSystem(relationSystemName1);
            ItSystemHelper.createLocalSystem(relationSystemName1);
            ItSystemHelper.createSystem(relationSystemName2);
            ItSystemHelper.createLocalSystem(relationSystemName2);
        }, testFixture.longRunningSetup());

        beforeEach(() => {
            testFixture.cleanupState();
            testFixture.enableLongRunningTest();
        });

        afterAll(() => {
            testFixture.disableLongRunningTest();
            testFixture.cleanupState();
        });

        it("User can create relations ",
            () => {
                loginHelper.loginAsGlobalAdmin()
                    .then(() => RelationHelper.createRelation(relationSystemName1,
                        relationSystemName2,
                        "testReference",
                        "testDescription"));

            });

    });

function createItSystemName() {
    return `SystemWithRelation${new Date().getTime()}`;
}