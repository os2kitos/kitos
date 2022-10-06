import login = require("../../../../Helpers/LoginHelper");
import ItSystemHelper = require("../../../../Helpers/SystemCatalogHelper");
import ItSystemUsageHelper = require("../../../../Helpers/SystemUsageHelper");
import ItSystemUsageMainPage = require("../../../../PageObjects/it-system/Usage/Tabs/ItSystemUsageMain.po");
import TestFixtureWrapper = require("../../../../Utility/TestFixtureWrapper");
import ItSystemUsageCommon = require("../../../../PageObjects/it-system/Usage/Tabs/ItSystemUsageCommon.po");
import Constants = require("../../../../Utility/Constants");

describe("User is able to view local it system main page information",
    () => {
        const consts = new Constants();

        const userCountInputs = {
            DefaultNull: { text: "" },
            Undecided: { text: " " },
            TenToFifty: { text: "10-50" }
        };

        const lifeCycleStatusInputs = {
            DefaultNull: { text: "" },
            Undecided: { text: " " },
            Operational: { text: "Under udfasning" }
        };

        var loginHelper = new login();
        var testFixture = new TestFixtureWrapper();

        var mainSystemName = createName("SystemUsageMain$");

        beforeAll(() => {
            loginHelper.loginAsGlobalAdmin()
                .then(() => ItSystemHelper.createSystem(mainSystemName))
                .then(() => ItSystemHelper.createLocalSystem(mainSystemName));
        });

        beforeEach(() => {
            testFixture.cleanupState();
            testFixture.enableLongRunningTest();
        });

        afterAll(() => {
            testFixture.disableLongRunningTest();
            testFixture.cleanupState();
        });

        

        it("User can view main page",
            () => {
                loginHelper.loginAsGlobalAdmin()
                    .then(() => ItSystemUsageHelper.openLocalSystem(mainSystemName))
                    .then(() => checkDefaultValues(mainSystemName));
            });

        it("Change UserCount from null to 10-50, then change to undecided",
            () => {
                loginHelper.loginAsGlobalAdmin()
                    .then(() => ItSystemUsageHelper.openLocalSystem(mainSystemName))
                    .then(() => ItSystemUsageHelper.validateSelectData(userCountInputs.DefaultNull.text, consts.mainUserCount))
                    .then(() => ItSystemUsageHelper.selectOption(userCountInputs.TenToFifty.text, consts.mainUserCount))
                    .then(() => browser.refresh())
                    .then(() => ItSystemUsageHelper.validateSelectData(userCountInputs.TenToFifty.text, consts.mainUserCount))
                    .then(() => ItSystemUsageHelper.selectOption(userCountInputs.Undecided.text, consts.mainUserCount))
                    .then(() => browser.refresh())
                    .then(() => ItSystemUsageHelper.validateSelectData(userCountInputs.Undecided.text, consts.mainUserCount));
            });

        it("Change LifeCycleStatus from null to Operational, then change to undecided",
            () => {
                loginHelper.loginAsGlobalAdmin()
                    .then(() => ItSystemUsageHelper.openLocalSystem(mainSystemName))
                    .then(() => ItSystemUsageHelper.validateSelectData(lifeCycleStatusInputs.DefaultNull.text, consts.mainLifeCycleStatus))
                    .then(() => ItSystemUsageHelper.selectOption(lifeCycleStatusInputs.Operational.text, consts.mainLifeCycleStatus))
                    .then(() => browser.refresh())
                    .then(() => ItSystemUsageHelper.validateSelectData(lifeCycleStatusInputs.Operational.text, consts.mainLifeCycleStatus))
                    .then(() => ItSystemUsageHelper.selectOption(lifeCycleStatusInputs.Undecided.text, consts.mainLifeCycleStatus))
                    .then(() => browser.refresh())
                    .then(() => ItSystemUsageHelper.validateSelectData(lifeCycleStatusInputs.Undecided.text, consts.mainLifeCycleStatus));
            });
    }
);


var itSystemUsageCommonPage = new ItSystemUsageCommon();

function createName(prefix: string) {
    return `${prefix}_${new Date().getTime()}`;
}

function checkDefaultValues(mainSystemName: string) {
    expect(ItSystemUsageMainPage.getHeaderName().getText()).toBe(mainSystemName + " - i Fælles Kommune");
    expect(ItSystemUsageMainPage.getLocalId().getAttribute("value")).toBe("");
    expect(ItSystemUsageMainPage.getLocalCallName().getAttribute("value")).toBe("");
    expect(ItSystemUsageMainPage.getNote().getAttribute("value")).toBe("");
    expect(ItSystemUsageMainPage.getVersion().getAttribute("value")).toBe("");
    expect(ItSystemUsageMainPage.getOwner().getAttribute("value")).toBe("Automatisk oprettet testbruger (GlobalAdmin)");
    expect(ItSystemUsageMainPage.getSystemName().getAttribute("value")).toBe(mainSystemName);
    expect(ItSystemUsageMainPage.getParentName().getAttribute("value")).toBe("");
    expect(ItSystemUsageMainPage.getPreviousName().getAttribute("value")).toBe("");
    expect(ItSystemUsageMainPage.getBelongsTo().getAttribute("value")).toBe("Fælles Kommune");
    expect(ItSystemUsageMainPage.getAccessModifier().getAttribute("value")).toBe("Offentlig");
    expect(ItSystemUsageMainPage.getDescription().getAttribute("value")).toBe("");
    expect(ItSystemUsageMainPage.getBusinessType().getAttribute("value")).toBe("");
    expect(ItSystemUsageMainPage.getArchiveDuty().getAttribute("value")).toBe("");
    expect(ItSystemUsageMainPage.getReferences().count()).toBe(1);
    expect(ItSystemUsageMainPage.getReferences().first().element(by.tagName("p")).getText()).toBe("Ingen referencer");
    expect(ItSystemUsageMainPage.getKLE().count()).toBe(1);
    expect(ItSystemUsageMainPage.getKLE().first().element(by.tagName("p")).getText()).toBe("Ingen KLE valgt");
}