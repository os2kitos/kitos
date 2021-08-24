import login = require("../../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import ItSystemHelper = require("../../../Helpers/SystemCatalogHelper");
import ItSystemReference = require("../../../PageObjects/it-system/Tabs/ItSystemFrontpage.po");
import Select2Helper = require("../../../Helpers/Select2Helper");

describe("Global Admin can",
    () => {
        const optionInputs = {
            Undecided: { text: " " },
            B: { text: "B" }
        };
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

        const verifyCommentAvailability = (available: boolean) => {
            if (available) {
                expect(pageObject.getArchiveDutyCommentInputAsArray()).toBeArrayOfSize(1);
            } else {
                expect(pageObject.getArchiveDutyCommentInputAsArray()).toBeEmptyArray();
            }
        };

        const selectArchiveDuty = (selection: string) => {
            return Select2Helper.selectWithNoSearch(selection, pageObject.getArchiveDutyRecommendationElementId());
        };

        const enterComment = (newComment: string) => {
            return pageObject.getArchiveDutyCommentInput().sendKeys(newComment);
        };

        it("Edit Recommended ArchiveDuty and add comment", () => {
            var comment = `Comment${new Date().getTime()}`;

            return ItSystemHelper.openSystem(itSystemName)
                .then(() => verifyCommentAvailability(false))
                .then(() => selectArchiveDuty(optionInputs.B.text))
                .then(() => {
                    console.log("Comment field should now be available on the UI");
                    return verifyCommentAvailability(true);
                })
                .then(() => enterComment(comment))
                .then(() => selectArchiveDuty(optionInputs.Undecided.text))
                .then(() => {
                    console.log("Comment field should have been removed from the UI since user has moved back to 'undecided'");
                    return verifyCommentAvailability(false);
                });
        });
    });
