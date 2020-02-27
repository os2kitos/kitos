import login = require("../../../Helpers/LoginHelper");
import ItSystemHelper = require("../../../Helpers/SystemCatalogHelper");
import ItSystemUsageHelper = require("../../../Helpers/SystemUsageHelper");
import ItSystemUsageMainPage = require("../../../PageObjects/it-system/Usage/Tabs/ItSystemUsageMain.po");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");

describe("User is able to view local it system main page information",
    () => {
        var loginHelper = new login();
        var testFixture = new TestFixtureWrapper();

        var mainSystemName = createItSystemName();

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
    }
);

function createItSystemName() {
    return `SystemUsageMain${new Date().getTime()}`;
}

function checkDefaultValues(mainSystemName: string) {
    expect(ItSystemUsageMainPage.getHeaderName().getText()).toBe(mainSystemName + " i Fælles Kommune");
    expect(ItSystemUsageMainPage.getLocalId().getAttribute("value")).toBe("");
    expect(ItSystemUsageMainPage.getLocalCallName().getAttribute("value")).toBe("");
    expect(ItSystemUsageMainPage.getNote().getAttribute("value")).toBe("");
    expect(ItSystemUsageMainPage.getVersion().getAttribute("value")).toBe("");
    expect(ItSystemUsageMainPage.getOwner().getAttribute("value")).toBe("Automatisk oprettet testbruger (GlobalAdmin)");
    expect(ItSystemUsageMainPage.getSystemName().getAttribute("value")).toBe(mainSystemName);
    expect(ItSystemUsageMainPage.getParentName().getAttribute("value")).toBe("");
    expect(ItSystemUsageMainPage.getPreviousName().getAttribute("value")).toBe("");
    expect(ItSystemUsageMainPage.getBelongsTo().getAttribute("value")).toBe("Fælles Kommune");
    expect(ItSystemUsageMainPage.getAccessModifier().getAttribute("value")).toBe("Lokal");
    expect(ItSystemUsageMainPage.getDescription().getAttribute("value")).toBe("");
    expect(ItSystemUsageMainPage.getBusinessType().getAttribute("value")).toBe("");
    expect(ItSystemUsageMainPage.getArchiveDuty().getAttribute("value")).toBe("");
}