import Login = require("../../Helpers/LoginHelper");
import ItSystemInterface = require("../../PageObjects/It-system/Interfaces/itSystemInterface.po");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import InterfaceCatalogHelper = require("../../Helpers/InterfaceCatalogHelper");
import InterfaceHelper = require("../../Helpers/InterfaceHelper");
import ItSystemHelper = require("../../Helpers/SystemCatalogHelper");

describe("Only Global Administrator is able to create and fill out an interface", () => {
    var loginHelper = new Login();
    var pageObject = new ItSystemInterface();
    var testFixture = new TestFixtureWrapper();


    afterEach(() => {
        testFixture.cleanupState();
    });

    beforeAll(() => {
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.disableLongRunningTest();
    });

    

    it("Global Admin is able to create and fill out data on a interface", () => {
        var iName = createInterfaceName();
        var sysName = createItSystem();
        var sysInterface = "CSV";
        var access = "Lokal";
        var org = "Fælles Kommune";
        var dataType = "Sag";
        var data = createData();
        var version = createVersion();
        
        loginHelper.loginAsGlobalAdmin()
            .then(() => {
                console.log("Getting page");
                return pageObject.getPage();
            }).then(() => {
                console.log("Creating interface");
                return InterfaceCatalogHelper.createInterface(iName);
            }).then(() => {
                console.log("Creating IT system");
                return ItSystemHelper.createSystem(sysName);
            }).then(() => {
                console.log("Inserting data");
                return InterfaceHelper.writeDataToAllInputs(iName, data, sysName, sysInterface, access, org, dataType, version);
            }).then(() => {
                console.log("Verifying data");
                return InterfaceHelper.verifyDataWasSaved(data, sysName, sysInterface, access, org, dataType, version);
            });
    });

    function createInterfaceName() {
        return `Interface${new Date().getTime()}`;
    }

    function createItSystem() {
        return `ItSystem${new Date().getTime()}`;
    }

    function createData() {
        return `SawData${new Date().getTime()}`;
    }

    function createVersion() {
        return `${new Date().getTime()}`;
    }
});