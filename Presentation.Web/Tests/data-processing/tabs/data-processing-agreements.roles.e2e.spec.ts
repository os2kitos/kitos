"use strict";
import Login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingAgreementOverviewPageObject = require("../../PageObjects/Data-Processing/data-processing-agreement.overview.po");
import DataProcessingAgreementHelper = require("../../Helpers/DataProcessingAgreementHelper")

describe("Data processing agreement main detail tests", () => {

    const loginHelper = new Login();
    const pageObject = new DataProcessingAgreementOverviewPageObject();
    const testFixture = new TestFixtureWrapper();
    const dpaHelper = DataProcessingAgreementHelper;

    const createName = () => {
        return `Dpa${new Date().getTime()}`;
    }

    const roleName1 = "Standard Læserolle (læs)";
    const roleName2 = "Standard Skriverolle (skriv)";
    const globalAdminEmail = "local-global-admin-user@kitos.dk";
    const globalAdminName = "Automatisk oprettet testbruger (GlobalAdmin)";
    const localAdminEmail = "local-local-admin-user@kitos.dk";
    const localAdminName = "Automatisk oprettet testbruger (LocalAdmin)";

    beforeAll(() => {
        testFixture.enableLongRunningTest();
        loginHelper.loginAsLocalAdmin()
            .then(() => dpaHelper.checkAndEnableDpaModule());
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
                .then(() => dpaHelper.assignRole(roleName2, localAdminEmail))
                .then(() => verifyRoleAssigned(roleName1, globalAdminName, globalAdminEmail))
                .then(() => verifyRoleAssigned(roleName2, localAdminName, localAdminEmail))
                .then(() => dpaHelper.removeRole(roleName2, localAdminName))
                .then(() => verifyRoleAssignmentDoesNotExist(roleName2, localAdminName))
                .then(() => dpaHelper.editRole(roleName1, globalAdminName, roleName2, localAdminEmail))
                .then(() => verifyRoleAssigned(roleName2, localAdminName, localAdminEmail))
                .then(() => verifyRoleAssignmentDoesNotExist(roleName1, globalAdminName));
        });


    function verifyRoleAssigned(role: string, user: string, email: string) {
        console.log(`Expecting role: ${role} to be assigned to: ${user} with email: ${email}`);
        expect(getRowObjectText(role, user, 0)).toBe(role);
        expect(getRowObjectText(role, user, 1)).toBe(user);
        expect(getRowObjectText(role, user, 2)).toBe(email);
    }

    function getRowObjectText(role: string, user: string, objectNumber: number) {
        return pageObject.getRoleRow(role, user).then(row => row.all(by.tagName("td")).get(objectNumber).getText());
    }

    function verifyRoleAssignmentDoesNotExist(role: string, user: string) {
        console.log(`Expecting role: ${role} assigned to: ${user} to not exist`);
        pageObject.getRoleRow(role, user).then(result => {},
            error => expect(error.toString()).toBe(`Error: Found no items with role: ${role} and user: ${user}`));
    }

});