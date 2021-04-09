"use strict";
import Login = require("../../Helpers/LoginHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingRegistrationOverviewPageObject = require("../../PageObjects/Data-Processing/data-processing-registration.overview.po");
import DataProcessingRegistrationHelper = require("../../Helpers/DataProcessingRegistrationHelper")

describe("Data processing agreement role tab tests", () => {

    const loginHelper = new Login();
    const pageObject = new DataProcessingRegistrationOverviewPageObject();
    const testFixture = new TestFixtureWrapper();
    const dpaHelper = DataProcessingRegistrationHelper;

    const createName = () => {
        return `Dpa${new Date().getTime()}`;
    }

    const roleName1 = "Standard Læserolle (læs)";
    const roleName2 = "Standard Skriverolle (skriv)";
    const localAdminSearchPhrase = "LocalAdmin";
    const localAdminName = "Automatisk oprettet testbruger (LocalAdmin)";
    const apiUserSearchPhrase = "Api User";
    const apiUserName = "Automatisk oprettet testbruger (Api User)";

    beforeAll(() => {
        testFixture.enableLongRunningTest();
        loginHelper.loginAsLocalAdmin()
            .then(() => dpaHelper.checkAndEnableDpaModule());
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });


    it("Performing role assignment and removal",
        () => {
            var name = createName();

            dpaHelper.createAndOpenDataProcessingRegistration(name)
                .then(() => dpaHelper.goToRoles())
                .then(() => dpaHelper.assignRole(roleName1, apiUserSearchPhrase))
                .then(() => dpaHelper.assignRole(roleName2, localAdminSearchPhrase))
                .then(() => verifyRoleAssigned(roleName1, apiUserName))
                .then(() => verifyRoleAssigned(roleName2, localAdminName))
                .then(() => dpaHelper.removeRole(roleName2, localAdminName))
                .then(() => verifyRoleAssignmentDoesNotExist(roleName2, localAdminName))
                .then(() => dpaHelper.editRole(roleName1, apiUserName, roleName2, localAdminSearchPhrase))
                .then(() => verifyRoleAssigned(roleName2, localAdminName))
                .then(() => verifyRoleAssignmentDoesNotExist(roleName1, apiUserName));
        });


    function verifyRoleAssigned(role: string, user: string) {
        console.log(`Expecting role: ${role} to be assigned to: ${user}`);
        expect(getRowObjectText(role, user, 0)).toBe(role);
        expect(getRowObjectText(role, user, 1)).toBe(user);
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