import login = require("../../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../../Utility/TestFixtureWrapper");
import ItSystemHelper = require("../../../Helpers/SystemCatalogHelper");
import RelationHelper = require("../../../Helpers/RelationHelper");
import RelationPage = require("../../../PageObjects/it-system/Usage/Tabs/ItSystemUsageRelation.po");
import InterfaceCatalogHelper = require("../../../Helpers/InterfaceCatalogHelper");
import ContractHelper = require("../../../Helpers/ContractHelper");
import LocalSystemNavigation = require("../../../Helpers/SideNavigation/LocalItSystemNavigation");

describe("User is able to create and view relation",
    () => {
        var loginHelper = new login();
        var testFixture = new TestFixtureWrapper();

        var frequencyType = "Dagligt";
        var description = "some description";
        var reference = "some reference";

        var relationSystemName1 = createItSystemName() + "1";
        var relationSystemName2 = createItSystemName() + "2";
        var interfaceName = createInterfaceName();
        var contractName = createContractName();

        // Data for edit test
        var frequencyTypeEdited = "Kvartal";
        var descriptionEdited = "some description edited";
        var referenceEdited = "some reference edited";
        var interfaceNameEdited = createInterfaceName();
        var contractNameEdited = createContractName();

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
                        frequencyType,
                        contractName,
                        reference,
                        description))
                    .then(() => checkForRelationPart(relationSystemName2))
                    .then(() => checkForRelationPart(interfaceName))
                    .then(() => checkForRelationPart(contractName))
                    .then(() => checkForDescription(relationSystemName2, description))
                    .then(() => checkForReference(relationSystemName2, reference))
                    .then(() => checkForFrequencyType(relationSystemName2, frequencyType))
                    .then(() => RelationPage.getRelationLink(relationSystemName2).click())
                    .then(() => LocalSystemNavigation.relationsPage())
                    .then(() => checkForRelationPart(relationSystemName1))
                    .then(() => checkForRelationPart(interfaceName))
                    .then(() => checkForRelationPart(contractName))
                    .then(() => checkForUsedByDescription(relationSystemName1, description))
                    .then(() => checkForUsedByReference(relationSystemName1, reference))
                    .then(() => checkForUsedByFrequencyType(relationSystemName1, frequencyType))
                    .then(() => RelationHelper.editRelation(relationSystemName1,
                        relationSystemName2,
                        interfaceNameEdited,
                        frequencyTypeEdited,
                        contractNameEdited,
                        referenceEdited,
                        descriptionEdited))
                    .then(() => checkForRelationPart(relationSystemName2))
                    .then(() => checkForRelationPart(interfaceNameEdited))
                    .then(() => checkForRelationPart(contractNameEdited))
                    .then(() => checkForDescription(relationSystemName2, descriptionEdited))
                    .then(() => checkForReference(relationSystemName2, referenceEdited))
                    .then(() => checkForFrequencyType(relationSystemName2, frequencyTypeEdited))
                    .then(() => RelationHelper.deleteRelation(relationSystemName1, relationSystemName2))
                    .then(() => checkIfRelationIsDeleted(relationSystemName2)); 

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

function checkIfRelationIsDeleted(name: string) {
    expect(RelationPage.getRelationLink(name).isPresent()).toBe(false);
}

function checkForRelationPart(name: string) {
    expect(RelationPage.getRelationLink(name).getText()).toMatch(name);
}

function checkForDescription(systemName: string, description: string) {
    expect(RelationPage.getDescription(systemName).getText()).toMatch(description);
}

function checkForReference(systemName: string, reference: string) {
    expect(RelationPage.getReference(systemName).getText()).toMatch(reference);
}

function checkForFrequencyType(systemName: string, frequencyType: string) {
    expect(RelationPage.getFrequencyType(systemName).getText()).toMatch(frequencyType);
}


function checkForUsedByDescription(systemName: string, description: string) {
    expect(RelationPage.getUsedByDescription(systemName).getText()).toMatch(description);
}

function checkForUsedByReference(systemName: string, reference: string) {
    expect(RelationPage.getUsedByReference(systemName).getText()).toMatch(reference);
}

function checkForUsedByFrequencyType(systemName: string, frequencyType: string) {
    expect(RelationPage.getUsedByFrequencyType(systemName).getText()).toMatch(frequencyType);
}