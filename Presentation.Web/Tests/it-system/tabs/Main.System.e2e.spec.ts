import login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import ItSystemHelper = require("../../Helpers/SystemCatalogHelper");

describe("Global Admin can",
    () => {
        var loginHelper = new login();
        var testFixture = new TestFixtureWrapper();
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

        it("Edit Recommended ArchiveDuty and add comment", () => {
            var comment = `Comment${new Date().getTime()}`;

            const verifyCommentAvailability = (available: boolean) => {

            };

            const selectArchiveDuty = (selection: string) => {

            };

            const enterComment = (newComment: string) => {

            };

            return loginHelper.loginAsGlobalAdmin()
                .then(() => ItSystemHelper.openSystem(itSystemName))
                .then(() => verifyCommentAvailability(false))
                .then(() => selectArchiveDuty("B"))
                .then(() => verifyCommentAvailability(true))
                .then(() => enterComment(comment))
                .then(() => selectArchiveDuty(null))
                .then(() => verifyCommentAvailability(false));
        });
    });
