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
        var startDate = DateHelper.getDateWithOffsetFromTodayAsString(1);
        var endDate = DateHelper.getDateWithOffsetFromTodayAsString(2);
        var email = getRandomEmail();
        var subjectOfAdviceToBeDeleted = getRandomText("1");
        var subjectOfImmediateAdvice = getRandomText("2");
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
                    .then(() => adviceHelper.createNewRepetitionAdvice(email, startDate, endDate, subjectOfAdviceToBeDeleted, "Uge"))
                    .then(() => verifyAdviceWasCreated(subjectOfAdviceToBeDeleted))
                    .then(() => adviceHelper.createNewInstantAdvice(email, subjectOfImmediateAdvice))
                    .then(() => verifyAdviceWasCreated(subjectOfImmediateAdvice))
                    .then(() => adviceHelper.deactivateAdvice(subjectOfAdviceToBeDeleted))
                    .then(() => adviceHelper.deleteAdvice(subjectOfAdviceToBeDeleted))
                    .then(() => verifyAdviceWasDeleted(subjectOfAdviceToBeDeleted));
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
