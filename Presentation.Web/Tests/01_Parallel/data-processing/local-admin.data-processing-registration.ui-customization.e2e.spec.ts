import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingRegistrationHelper = require("../../Helpers/DataProcessingRegistrationHelper")
import { UiCustomizationTestHelper } from "../../Helpers/ui-customization-test-helper";

describe("Local admin is able customize the DataProcessingRegistration UI", () => {

    var testFixture = new TestFixtureWrapper();
    const dpaHelper = DataProcessingRegistrationHelper;

    beforeAll(() => {
        testFixture.enableLongRunningTest();
    });

    afterAll(() => {
        testFixture.cleanupState();
        testFixture.disableLongRunningTest();
    });

    afterEach(() => {
        testFixture.cleanupState();
    });

    it("Disabling tabs will hide the tabs on the DataProcessingRegistration details page", () => {

        const helper = new UiCustomizationTestHelper({
            createEntity: name => dpaHelper.createDataProcessingRegistration(name),
            navigateToEntity: name => dpaHelper.goToSpecificDataProcessingRegistration(name),
            localAdminArea: "data-processing",
            module: "DataProcessingRegistrations"
        });

        return helper.setupUserAndOrg()
            .then(() => helper.testTabCustomization("DataProcessingRegistrations.roles", "data-processing.edit-registration.roles"))
            .then(() => helper.testTabCustomization("DataProcessingRegistrations.references", "data-processing.edit-registration.reference"))
            .then(() => helper.testTabCustomization("DataProcessingRegistrations.notifications", "data-processing.edit-registration.advice"));
    });

    it("Disabling fields will hide the fields on the DataProcessingRegistration details page", () => {

        const helper = new UiCustomizationTestHelper({
            createEntity: name => dpaHelper.createDataProcessingRegistration(name),
            navigateToEntity: name => dpaHelper.goToSpecificDataProcessingRegistration(name),
            localAdminArea: "data-processing",
            module: "DataProcessingRegistrations"
        });

        return helper.setupUserAndOrg()
            .then(() => helper.testFieldCustomization("DataProcessingRegistrations.oversight.scheduledInspectionDate", "data-processing.edit-registration.oversight", "scheduledInspection"));
    });
});


