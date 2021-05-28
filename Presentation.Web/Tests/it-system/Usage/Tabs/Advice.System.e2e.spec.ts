import login = require("../../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import ItSystemHelper = require("../../../Helpers/SystemCatalogHelper");
import AdviceHelper = require("../../../Helpers/AdviceHelper");
import WaitTimers = require("../../../Utility/WaitTimers");
import DateHelper = require("../../../Helpers/GetDateHelper");

describe("Is able to create advice and delete advice",
    () => {
        var loginHelper = new login();
        var itSystemName = createItSystemName();
        var startDate = DateHelper.getTodayAsString();
        var endDate = DateHelper.getTodayAsString();
        var email = getRandomEmail();
        var subjectText1 = getRandomText("1");
        var subjectText2 = getRandomText("2");
        var adviceHelper = new AdviceHelper();
        var waitUpTo = new WaitTimers();
        var testFixture = new TestFixtureWrapper();

        beforeAll(() => {
            loginHelper.loginAsGlobalAdmin()
                .then(() => ItSystemHelper.createSystemAndActivateLocally(itSystemName));
        });

        beforeEach(() => {
            testFixture.enableLongRunningTest();
        });

        afterAll(() => {
            testFixture.disableLongRunningTest();
            testFixture.cleanupState();
        });

        it("Is able to create a new instant and repetition advice and delete a advice",
            () => {
                adviceHelper.goToSpecificItSystemAdvice(itSystemName)
                    .then(() => adviceHelper.createNewRepetitionAdvice(email, startDate, endDate, subjectText1, "Uge"))
                    .then(() => verifyAdviceWasCreated(subjectText1))
                    .then(() => adviceHelper.createNewInstantAdvice(email, subjectText2))
                    .then(() => verifyAdviceWasCreated(subjectText2))
                    .then(() => adviceHelper.deactivateAdvice(subjectText1))
                    .then(() => adviceHelper.deleteAdvice(subjectText1))
                    .then(() => verifyAdviceWasDeleted(subjectText1));
            });

        function createItSystemName() {
            return `ItSystemAdviceTest${new Date().getTime()}`;
        }

        function getRandomEmail() {
            return `ItSystemAdviceTest@${new Date().getTime()}.com`;
        }

        function getRandomText(text: string) {
            return `ItSystemAdviceText${new Date().getTime()}-${text}`;
        }

        function verifyAdviceWasCreated(subjectName: string) {
            console.log(`waiting for ${subjectName} to appear`);
            return browser.wait(element(by.xpath(`.//*[@id="mainGrid"]//span[text() = ${subjectName}]//text()`)).isPresent(),
                waitUpTo.twentySeconds);
        }

        function verifyAdviceWasDeleted(subjectName: string) {
            console.log(`verifying that ${subjectName} has been deleted`);
            return expect(browser.wait(
                element(by.xpath(`.//*[@id="mainGrid"]//span[text() = ${subjectName}]//text()`)).isPresent(),
                waitUpTo.twentySeconds)).toBe(false);
        }
    });
