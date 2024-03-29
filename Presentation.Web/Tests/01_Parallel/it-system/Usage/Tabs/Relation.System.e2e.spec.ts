﻿import login = require("../../../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../../../Utility/TestFixtureWrapper");
import ItSystemHelper = require("../../../../Helpers/SystemCatalogHelper");
import RelationHelper = require("../../../../Helpers/RelationHelper");
import RelationPage = require("../../../../PageObjects/it-system/Usage/Tabs/ItSystemUsageRelation.po");
import InterfaceCatalogHelper = require("../../../../Helpers/InterfaceCatalogHelper");
import ContractHelper = require("../../../../Helpers/ContractHelper");
import LocalSystemNavigation = require("../../../../Helpers/SideNavigation/LocalItSystemNavigation");
import ContractNavigation = require("../../../../Helpers/SideNavigation/ContractNavigation");
import ContractSystemPage = require("../../../../PageObjects/It-contract/Tabs/ContractItSystem.po");

describe("User is able to create and view relation",
    () => {
        var loginHelper = new login();
        var testFixture = new TestFixtureWrapper();

        var frequencyType = "Dagligt";
        var description = "some description";
        var reference = "some reference";

        var relationSystemName1 = createName("RelationSystem1");
        var relationSystemName2 = createName("RelationSystem2");
        var interfaceName = createName("RelationInterface");
        var contractName = createName("RelationContract");

        // Data for edit test
        var frequencyTypeEdited = "Kvartal";
        var descriptionEdited = "some description edited";
        var referenceEdited = "some reference edited";

        var expectedRelationCount = "1";

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
            testFixture.enableLongRunningTest();
        });

        afterAll(() => {
            testFixture.disableLongRunningTest();
            testFixture.cleanupState();
        });

        it("User can create relations ",
            () => {
                RelationHelper.createRelation(relationSystemName1,
                    relationSystemName2,
                    interfaceName,
                    frequencyType,
                    contractName,
                    reference,
                    description)
                    .then(() => checkForRelationPart(relationSystemName2))
                    .then(() => checkForRelationPart(interfaceName))
                    .then(() => checkForRelationPart(contractName))
                    .then(() => checkForDescription(relationSystemName2, description))
                    .then(() => checkForReference(relationSystemName2, reference))
                    .then(() => checkForFrequencyType(relationSystemName2, frequencyType))
                    .then(() => RelationPage.getRelationLink(relationSystemName2).click())
                    .then(() => browser.waitForAngular())
                    .then(() => LocalSystemNavigation.relationsPage())
                    .then(() => checkForRelationPart(relationSystemName1))
                    .then(() => checkForRelationPart(interfaceName))
                    .then(() => checkForRelationPart(contractName))
                    .then(() => checkForUsedByDescription(relationSystemName1, description))
                    .then(() => checkForUsedByReference(relationSystemName1, reference))
                    .then(() => checkForUsedByFrequencyType(relationSystemName1, frequencyType))
                    .then(() => checkContractOverviewToShowRelationCount(contractName, expectedRelationCount))
                    .then(() => ContractHelper.openContract(contractName))
                    .then(() => ContractNavigation.openSystemsPage())
                    .then(() => browser.waitForAngular())
                    .then(() => checkContractForRelationPart(relationSystemName1))
                    .then(() => checkContractForRelationPart(relationSystemName2))
                    .then(() => checkContractForRelationPart(interfaceName))
                    .then(() => checkContractForDescription(relationSystemName1, description))
                    .then(() => checkContractForReference(relationSystemName1, reference))
                    .then(() => checkContractForFrequencyType(relationSystemName1, frequencyType))
                    .then(() => RelationHelper.editRelation(relationSystemName1,
                        relationSystemName2,
                        interfaceName,
                        frequencyTypeEdited,
                        contractName,
                        referenceEdited,
                        descriptionEdited))
                    .then(() => checkForRelationPart(relationSystemName2))
                    .then(() => checkForRelationPart(interfaceName))
                    .then(() => checkForRelationPart(contractName))
                    .then(() => checkForDescription(relationSystemName2, descriptionEdited))
                    .then(() => checkForReference(relationSystemName2, referenceEdited))
                    .then(() => checkForFrequencyType(relationSystemName2, frequencyTypeEdited))
                    .then(() => RelationHelper.deleteRelation(relationSystemName1, relationSystemName2))
                    .then(() => checkIfRelationIsDeleted(relationSystemName2));

            });
    });

function createName(prefix: string) {
    return `${prefix}_${new Date().getTime()}`;
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

function checkContractForRelationPart(name: string) {
    expect(ContractSystemPage.getElementByLink(name).getText()).toMatch(name);
}

function checkContractForDescription(systemName: string, description: string) {
    expect(ContractSystemPage.getDescription(systemName).getText()).toMatch(description);
}

function checkContractForReference(systemName: string, reference: string) {
    expect(ContractSystemPage.getReference(systemName).getText()).toMatch(reference);
}

function checkContractForFrequencyType(systemName: string, frequencyType: string) {
    expect(ContractSystemPage.getFrequencyType(systemName).getText()).toMatch(frequencyType);
}

function checkContractOverviewToShowRelationCount(contractName: string, expectedCount: string) {
    expect(ContractHelper.getRelationCountFromContractName(contractName)).toBe(expectedCount);
}