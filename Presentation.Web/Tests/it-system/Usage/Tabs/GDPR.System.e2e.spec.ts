import login = require("../../../Helpers/LoginHelper");
import ItSystemCatalogHelper = require("../../../Helpers/SystemCatalogHelper");
import itSystemHelper = require("../../../Helpers/SystemUsageHelper");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import ItSystemUsageGDPR = require("../../../PageObjects/It-system/Usage/Tabs/ItSystemUsageGDPR.po");
import LocalSystemNavigation = require("../../../Helpers/SideNavigation/LocalItSystemNavigation");
import Constants = require("../../../Utility/Constants")
import CssHelper = require("../../../Object-wrappers/CSSLocatorHelper")
import NavigationHelper = require("../../../Utility/NavigationHelper");

describe("User is able to", () => {
    var navigationHelper = new NavigationHelper();
    var loginHelper = new login();
    var testFixture = new TestFixtureWrapper();
    var itSystem1 = createItSystemName();
    var consts = new Constants();
    var cssHelper = new CssHelper();

    beforeAll(() => {
        loginHelper.loginAsGlobalAdmin();
    });

    beforeEach(() => {
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.disableLongRunningTest();
        testFixture.cleanupState();
    });

    it("Select and remove a datalevel on a it system",
        () => {
            ItSystemCatalogHelper.createSystem(itSystem1)
                .then(() => ItSystemCatalogHelper.createLocalSystem(itSystem1))
                .then(() => itSystemHelper.openLocalSystem(itSystem1))
                .then(() => LocalSystemNavigation.openGDPRPage())
                .then(() => ItSystemUsageGDPR.getRegularDataLevelCheckBox().click())
                .then(() => expectCheckboxVisibilityToBe(consts.defaultPersonalSensitivData1,false))
                .then(() => ItSystemUsageGDPR.getSensitiveDataLevelCheckBox().click())
                .then(() => ItSystemUsageGDPR.getLegalDataLevelCheckBox().click())
                .then(() => navigationHelper.refreshPage())
                .then(() => expectCheckboxVisibilityToBe(consts.defaultPersonalSensitivData1, true))
                .then(() => expectCheckboxValue(consts.dataLevelTypeNoneCheckbox, false))
                .then(() => expectCheckboxValue(consts.dataLevelTypeRegularCheckbox, true))
                .then(() => expectCheckboxValue(consts.dataLevelTypeSensitiveCheckbox, true))
                .then(() => expectCheckboxValue(consts.dataLevelTypeLegalCheckbox, true));
        });

    function createItSystemName() {
        return `Gdprtest${new Date().getTime()}`;
    }

    function expectCheckboxValue(checkBoxDataElementType: string, toBe: boolean) {
        console.log("Checking value for " + checkBoxDataElementType + " value to be " + toBe); 
        return expect(element(cssHelper.byDataElementType(checkBoxDataElementType)).isSelected()).toBe(toBe);
    }

    function expectCheckboxVisibilityToBe(checkBoxDataElementType, toBe: boolean) {
        console.log("Checking value for " + checkBoxDataElementType + " value to be " + toBe);
        return expect(element(cssHelper.byDataElementType(checkBoxDataElementType)).isPresent()).toBe(toBe);
    }

});
