import login = require("../../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import ItSystemHelper = require("../../../Helpers/SystemCatalogHelper");
import RelationHelper = require("../../../Helpers/RelationHelper");
import RelationPage = require("../../../PageObjects/it-system/Usage/Tabs/ItSystemUsageRelation.po");
import InterfaceCatalogHelper = require("../../../Helpers/InterfaceCatalogHelper");
import ContractHelper = require("../../../Helpers/ContractHelper");

describe("User is able to create and view relation",
    () => {
        var loginHelper = new login();
        var testFixture = new TestFixtureWrapper();

        var relationSystemName1 = createItSystemName() + "1";
        var relationSystemName2 = createItSystemName() + "2";
        var interfaceName = createInterfaceName();
        var contractName = createContractName();

        beforeAll(() => {
            loginHelper.loginAsGlobalAdmin()
                .then(() => ItSystemHelper.createSystem(relationSystemName1))
                .then(() => ItSystemHelper.createLocalSystem(relationSystemName1))
                .then(() => ItSystemHelper.createSystem(relationSystemName2))
                .then(() => ItSystemHelper.createLocalSystem(relationSystemName2))
                .then(() => InterfaceCatalogHelper.createInterface(interfaceName))
                .then(() => InterfaceCatalogHelper.bindInterfaceToSystem(relationSystemName2, interfaceName))
                .then(() => ContractHelper.createContract(contractName));
            
        }, testFixture.longRunningSetup());

        beforeEach(() => {
            testFixture.cleanupState();
            testFixture.enableLongRunningTest();
        });

        afterAll(() => {
            testFixture.disableLongRunningTest();
            testFixture.cleanupState();
        });

        it("User can create relations ",
            () => {
                loginHelper.loginAsGlobalAdmin()
                    .then(() => RelationHelper.createRelation(relationSystemName1,
                        relationSystemName2,
                        interfaceName,
                        "Dagligt",
                        contractName,
                        "testReference",
                        "testDescription"))
                    .then(() => expect(RelationPage.getRelationExhibitSystem(relationSystemName2).getText()).toMatch(relationSystemName2))
                    .then(() => expect(RelationPage.getRelationInterface(interfaceName).getText()).toMatch(interfaceName))
                    .then(() => expect(RelationPage.getRelationContract(contractName).getText()).toMatch(contractName));

            });

    });

function createItSystemName() {
    return `SystemWithRelation${new Date().getTime()}`;
}

function createInterfaceName() {
    return `InterfaceForRelation${new Date().getTime()}`;
}

function createContractName() {
    return `ContractForRelation${new Date().getTime()}`;
}