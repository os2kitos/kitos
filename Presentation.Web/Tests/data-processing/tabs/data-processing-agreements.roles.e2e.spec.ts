"use strict";
import Login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingAgreementOverviewPageObject = require("../../PageObjects/Data-Processing/data-processing-agreement.overview.po");
import WaitTimers = require("../../Utility/WaitTimers");
import DataProcessingAgreementHelper = require("../../Helpers/DataProcessingAgreementHelper")

describe("Data processing agreement main detail tests", () => {

    const loginHelper = new Login();
    const pageObject = new DataProcessingAgreementOverviewPageObject();
    const testFixture = new TestFixtureWrapper();
    const waitUpTo = new WaitTimers();
    const ec = protractor.ExpectedConditions;
    const dpaHelper = DataProcessingAgreementHelper;

    const createName = () => {
        return `Dpa${new Date().getTime()}`;
    }

    const roleName1 = "Standard Læserolle (læs)";
    const roleName2 = "Standard Skriverolle (skriv)";
    const globalAdminEmail = "local-global-admin-user@kitos.dk";
    const globalAdminName = "Automatisk oprettet testbruger (GlobalAdmin)";

    beforeAll(() => {
        
        testFixture.enableLongRunningTest();
        loginHelper.loginAsGlobalAdmin();
        //.then(() => dpaHelper.checkAndEnableDpaModule());
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });


    it("Creating and renaming data processing agreement",
        () => {
            var name = createName();

            dpaHelper.createDataProcessingAgreement(name)
                .then(() => pageObject.findSpecificDpaInNameColumn(name))
                .then(() => dpaHelper.goToSpecificDataProcessingAgreement(name))
                .then(() => dpaHelper.goToRoles())
                .then(() => dpaHelper.assignRole(roleName1, globalAdminEmail))
                .then(() => dpaHelper.assignRole(roleName2, globalAdminEmail))
                .then(() => verifyRoleAssigned(roleName1, globalAdminName, 0))
                .then(() => verifyRoleAssigned(roleName2, globalAdminName, 1))
                .then(() => dpaHelper.removeRole(1));
        });


    function verifyRoleAssigned(role: string, user: string, rowNumber: number) {
        console.log(`Expecting role: ${role} to be assigned to: ${user}`);
        expect(pageObject.getRoleRow(rowNumber).all(by.tagName("td")).get(0).getText()).toBe(role);
        expect(pageObject.getRoleRow(rowNumber).all(by.tagName("td")).get(1).getText()).toBe(user);
        expect(pageObject.getRoleRow(rowNumber).all(by.tagName("td")).get(2).getText()).toBe(globalAdminEmail);
    }

});