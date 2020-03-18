import login = require("../../../Helpers/LoginHelper");
import ItSystemCatalogHelper = require("../../../Helpers/SystemCatalogHelper");
import itSystemHelper = require("../../../Helpers/SystemUsageHelper");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import ItSystemUsageGDPR = require("../../../PageObjects/It-system/Usage/Tabs/ItSystemUsageGDPR.po");
import LocalSystemNavigation = require("../../../Helpers/SideNavigation/LocalItSystemNavigation");
import Constants = require("../../../Utility/Constants");
import CssHelper = require("../../../Object-wrappers/CSSLocatorHelper");
import Select2Helper = require("../../../Helpers/Select2Helper");
import NavigationHelper = require("../../../Utility/NavigationHelper");


describe("User is able to", () => {

    var loginHelper = new login();
    var testFixture = new TestFixtureWrapper();
    var itSystem1 = createItSystemName();
    var consts = new Constants();
    var cssHelper = new CssHelper();
    var navigationHelper = new NavigationHelper();

    beforeAll(() => {
        loginHelper.loginAsGlobalAdmin()
            .then(() => ItSystemCatalogHelper.createSystem(itSystem1))
            .then(() => ItSystemCatalogHelper.getActivationToggleButton(itSystem1).click())
            .then(() => itSystemHelper.openLocalSystem(itSystem1));
    }, testFixture.longRunningSetup());

    beforeEach(() => {
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.disableLongRunningTest();
        testFixture.cleanupState();
    });


    it("Able to select a HostedAt value on a it system",
        () => {
            var hostedValue1 = consts.hostedAtValueNone;
            var hostedValue2 = consts.hostedAtValueExternal;
            var hostedValue3 = consts.hostedAtValueOnPremise;

            LocalSystemNavigation.openGDPRPage()
                .then(() => selectHostedAtValue(hostedValue1))
                .then(() => ItSystemUsageGDPR.refreshPage())
                .then(() => expectHostedAtToEqual(hostedValue1))
                .then(() => selectHostedAtValue(hostedValue2))
                .then(() => ItSystemUsageGDPR.refreshPage())
                .then(() => expectHostedAtToEqual(hostedValue2))
                .then(() => selectHostedAtValue(hostedValue3))
                .then(() => ItSystemUsageGDPR.refreshPage())
                .then(() => expectHostedAtToEqual(hostedValue3));
        });


    it("Select and remove a datalevel on a it system",
        () => {
            ItSystemUsageGDPR.getRegularDataLevelCheckBox().click()
                .then(() => expectCheckboxVisibilityToBe(consts.defaultPersonalSensitivData1, false))
                .then(() => ItSystemUsageGDPR.getSensitiveDataLevelCheckBox().click())
                .then(() => ItSystemUsageGDPR.getLegalDataLevelCheckBox().click())
                .then(() => ItSystemUsageGDPR.refreshPage())
                .then(() => expectCheckboxVisibilityToBe(consts.defaultPersonalSensitivData1, true))
                .then(() => expectCheckboxValue(consts.dataLevelTypeNoneCheckbox, false))
                .then(() => expectCheckboxValue(consts.dataLevelTypeRegularCheckbox, true))
                .then(() => expectCheckboxValue(consts.dataLevelTypeSensitiveCheckbox, true))
                .then(() => expectCheckboxValue(consts.dataLevelTypeLegalCheckbox, true));
        });

    function createItSystemName() {
        return `SystemUsageMain${new Date().getTime()}`;
    }

    function expectCheckboxValue(checkBoxDataElementType: string, toBe: boolean) {
        console.log("Checking value for " + checkBoxDataElementType + " value to be " + toBe);
        return expect(element(cssHelper.byDataElementType(checkBoxDataElementType)).isSelected()).toBe(toBe);
    }

    function expectCheckboxVisibilityToBe(checkBoxDataElementType, toBe: boolean) {
        console.log("Checking value for " + checkBoxDataElementType + " value to be " + toBe);
        return expect(element(cssHelper.byDataElementType(checkBoxDataElementType)).isPresent()).toBe(toBe);
    }

    function selectHostedAtValue(value: string) {
        return Select2Helper.selectWithNoSearch(value, consts.hostedAtSelect2Id);
    }

    function expectHostedAtToEqual(expectedValue: string) {
        console.log("Expecting hostedAt to equal " + expectedValue);
        return expect(Select2Helper.getData(consts.hostedAtSelect2Id).getText()).toEqual(expectedValue);
    }

});
