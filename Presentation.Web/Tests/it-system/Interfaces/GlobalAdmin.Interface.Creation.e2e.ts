import Login = require("../../Helpers/LoginHelper");
import ItSystemInterface = require("../../PageObjects/It-system/Interfaces/itSystemInterface.po");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import InterfaceCatalogHelper = require("../../Helpers/InterfaceCatalogHelper");
import ItSystemHelper = require("../../Helpers/SystemCatalogHelper");

describe("Only Global Administrator is able to create and fill out an interface", () => {
    var loginHelper = new Login();
    var pageObject = new ItSystemInterface();
    var testFixture = new TestFixtureWrapper();
    var interfaceCatalogHelper = new InterfaceCatalogHelper();


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
        var DataType = "Sag";
        var data = getRandomData();
        
        loginHelper.loginAsGlobalAdmin()
            .then(() => {
                console.log("Getting page");
                return pageObject.getPage();
            }).then(() => {
                console.log("Creating interface");
                return interfaceCatalogHelper.createInterface(iName);
            }).then(() => {
                console.log("Creating IT system");
                return ItSystemHelper.createSystem(sysName);
            }).then(() => {
                console.log("Inserting data");
                return interfaceCatalogHelper.insertRandomDataToInterface(iName, data, sysName, sysInterface, access, org, DataType);
            }).then(() => {
                console.log("Verifying data");
                return interfaceCatalogHelper.verifyRandomDataToInterface(data, sysName, sysInterface, access, org, DataType);
            });
    });

    function createInterfaceName() {
        return "Interface" + new Date().getTime();
    }

    function createItSystem() {
        return "ItSystem" + new Date().getTime();
    }

    function getRandomData() {
        return "SawData" + new Date().getTime();
    }
});