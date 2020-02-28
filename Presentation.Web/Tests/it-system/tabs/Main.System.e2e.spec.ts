import login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import ItSystemHelper = require("../../Helpers/SystemCatalogHelper");
import ItSystemReference = require("../../PageObjects/it-system/Tabs/ItSystemFrontpage.po");

describe("Global Admin can",
    () => {
        var loginHelper = new login();
        var testFixture = new TestFixtureWrapper();
        var pageObject = new ItSystemReference();

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
                if (available) {
                    expect(pageObject.getArchiveDutyCommentAsList()).toBeArrayOfSize(1);
                } else {
                     expect(pageObject.getArchiveDutyCommentAsList()).toBeEmptyArray();
                }
            };

            const selectArchiveDuty = (selection: string) => {

            };

            const enterComment = (newComment: string) => {
                return pageObject.getArchiveDutyCommentInput().sendKeys(newComment);
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
